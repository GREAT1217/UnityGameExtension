using System.Collections.Generic;
using UnityEngine;

namespace GameExtension.Editor
{
    public abstract class BatchEditHelperBase
    {
        protected readonly List<GameObject> m_TargetObjectObjects;

        public abstract string HelperName
        {
            get;
        }

        protected BatchEditHelperBase(List<GameObject> targetObjects)
        {
            m_TargetObjectObjects = targetObjects;
        }

        public abstract void CustomEditGUI();
    }
}
