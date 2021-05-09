using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhubarbEngine;
namespace RhubarbEngine.Managers
{
    public interface IManager
    {
        IManager initialize(Engine engine);
    }
}
