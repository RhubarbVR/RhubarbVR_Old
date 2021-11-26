using System;
using System.Collections.Generic;

namespace RNumerics
{
	public class ColorMap
	{
		struct ColorPoint
		{
			public float t;
			public Colorf c;
		}

        readonly List<ColorPoint> _points = new List<ColorPoint>();
		Interval1d _validRange;

		public ColorMap()
		{
			_validRange = Interval1d.Empty;
		}

		public ColorMap(float[] t, Colorf[] c)
		{
			_validRange = Interval1d.Empty;
			for (var i = 0; i < t.Length; ++i)
            {
                AddPoint(t[i], c[i]);
            }
        }

		public void AddPoint(float t, Colorf c)
		{
			var cp = new ColorPoint() { t = t, c = c };
			if (_points.Count == 0)
			{
				_points.Add(cp);
				_validRange.Contain(t);
			}
			else if (t < _points[0].t)
			{
				_points.Insert(0, cp);
				_validRange.Contain(t);
			}
			else
			{
				for (var k = 0; k < _points.Count; ++k)
				{
					if (_points[k].t == t)
					{
						_points[k] = cp;
						return;
					}
					else if (_points[k].t > t)
					{
						_points.Insert(k, cp);
						return;
					}
				}
				_points.Add(cp);
				_validRange.Contain(t);
			}
		}




		public Colorf Linear(float t)
		{
			if (t <= _points[0].t)
            {
                return _points[0].c;
            }

            var N = _points.Count;
			if (t >= _points[N - 1].t)
            {
                return _points[N - 1].c;
            }

            for (var k = 1; k < _points.Count; ++k)
			{
				if (_points[k].t > t)
				{
					ColorPoint prev = _points[k - 1], next = _points[k];
                    var a = (t - prev.t) / (next.t - prev.t);
                    return ((1.0f - a) * prev.c) + (a * next.c);
				}
			}
			return _points[N - 1].c;  // should never get here...
		}


	}
}
