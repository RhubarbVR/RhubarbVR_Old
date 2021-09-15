using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Veldrid;

namespace RhubarbEngine.VirtualReality.OpenVR
{
	internal class OpenVRMirrorTexture : IDisposable
	{
		private readonly List<IDisposable> _disposables = new();
		private readonly Dictionary<OutputDescription, TextureBlitter> _blitters
			= new();

		private readonly OpenVRContext _context;
		private ResourceSet _leftSet;
		private ResourceSet _rightSet;

		public OpenVRMirrorTexture(OpenVRContext context)
		{
			_context = context;
		}

		public void Render(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
		{
			cl.SetFramebuffer(fb);
			var blitter = GetBlitter(fb.OutputDescription);

			switch (source)
			{
				case MirrorTextureEyeSource.BothEyes:
                    var width = fb.Width * 0.5f;
					cl.SetViewport(0, new Viewport(0, 0, width, fb.Height, 0, 1));
					BlitLeftEye(cl, blitter, width / fb.Height);
					cl.SetViewport(0, new Viewport(width, 0, width, fb.Height, 0, 1));
					BlitRightEye(cl, blitter, width / fb.Height);
					break;
				case MirrorTextureEyeSource.LeftEye:
					BlitLeftEye(cl, blitter, (float)fb.Width / fb.Height);
					break;
				case MirrorTextureEyeSource.RightEye:
					BlitRightEye(cl, blitter, (float)fb.Width / fb.Height);
					break;
			}

			cl.SetFullViewports();
		}

		private void BlitLeftEye(CommandList cl, TextureBlitter blitter, float viewportAspect)
		{
            GetSampleRatio(_context.LeftEyeFramebuffer, viewportAspect, out var minUV, out var maxUV);
			var leftEyeSet = GetLeftEyeSet(blitter.ResourceLayout);
			blitter.Render(cl, leftEyeSet, minUV, maxUV);
		}

		private void BlitRightEye(CommandList cl, TextureBlitter blitter, float viewportAspect)
		{
            GetSampleRatio(_context.RightEyeFramebuffer, viewportAspect, out var minUV, out var maxUV);
			var rightEyeSet = GetRightEyeSet(blitter.ResourceLayout);
			blitter.Render(cl, rightEyeSet, minUV, maxUV);
		}

		private static void GetSampleRatio(Framebuffer eyeFB, float viewportAspect, out Vector2 minUV, out Vector2 maxUV)
		{
			var eyeWidth = eyeFB.Width;
			var eyeHeight = eyeFB.Height;

			uint sampleWidth, sampleHeight;
			if (viewportAspect > 1)
			{
				sampleWidth = eyeWidth;
				sampleHeight = (uint)(eyeWidth / viewportAspect);
			}
			else
			{
				sampleHeight = eyeHeight;
				sampleWidth = (uint)(eyeHeight / (1 / viewportAspect));
			}

			var sampleUVWidth = (float)sampleWidth / eyeWidth;
			var sampleUVHeight = (float)sampleHeight / eyeHeight;

			var max = (float)Math.Max(sampleUVWidth, sampleUVHeight);
			sampleUVWidth /= max;
			sampleUVHeight /= max;

			minUV = new Vector2(0.5f - (sampleUVWidth / 2f), 0.5f - (sampleUVHeight / 2f));
			maxUV = new Vector2(0.5f + (sampleUVWidth / 2f), 0.5f + (sampleUVHeight / 2f));
		}

		private ResourceSet GetLeftEyeSet(ResourceLayout rl)
		{
			if (_leftSet == null)
			{
				_leftSet = CreateColorTargetSet(rl, _context.LeftEyeFramebuffer);
			}

			return _leftSet;
		}

		private ResourceSet GetRightEyeSet(ResourceLayout rl)
		{
			if (_rightSet == null)
			{
				_rightSet = CreateColorTargetSet(rl, _context.RightEyeFramebuffer);
			}

			return _rightSet;
		}

		private ResourceSet CreateColorTargetSet(ResourceLayout rl, Framebuffer fb)
		{
			var factory = _context.GraphicsDevice.ResourceFactory;
			var target = fb.ColorTargets[0].Target;
			var view = factory.CreateTextureView(target);
			_disposables.Add(view);
			var rs = factory.CreateResourceSet(new ResourceSetDescription(rl, view, _context.GraphicsDevice.PointSampler));
			_disposables.Add(rs);

			return rs;
		}

		private TextureBlitter GetBlitter(OutputDescription outputDescription)
		{
            if (!_blitters.TryGetValue(outputDescription, out var ret))
			{
				ret = new TextureBlitter(
					_context.GraphicsDevice,
					_context.GraphicsDevice.ResourceFactory,
					outputDescription,
					srgbOutput: false);

				_blitters.Add(outputDescription, ret);
			}

			return ret;
		}

		public void Dispose()
		{
			foreach (var disposable in _disposables)
			{
				disposable.Dispose();
			}
			foreach (var kvp in _blitters)
			{
				kvp.Value.Dispose();
			}

			_leftSet?.Dispose();
			_rightSet?.Dispose();
		}
	}
}
