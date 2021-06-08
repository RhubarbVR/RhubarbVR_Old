using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbEngine.Render.Shader
{
    public abstract class ShaderPart
    {
        public virtual string TopCode
        {
            get
            {
                return @"
#version 450 Error
";
            } 
        }

        public string InjectedCode = "";

        private string userCode = @"
Error
";
        public virtual string UserCode
        {
            get
            {
                return userCode;
            }
            set
            {
                userCode = value;
            }
        }
        public virtual string getCode()
        {
            return TopCode + "\n"+ InjectedCode + "\n" + UserCode;
        }
    }
}
