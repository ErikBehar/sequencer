using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class MatchUiElement : UIBehaviour, ILayoutSelfController
    {
        [SerializeField]
        protected bool
            matchHorizontal = true;
        [SerializeField]
        protected bool
            matchVertical = true;

        [System.NonSerialized]
        private RectTransform
            m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }
        
        private DrivenRectTransformTracker m_Tracker;

        public RectTransform drivenRectTransform; 
        
        #region Unity Lifetime calls
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }
        
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }
        
        #endregion
        
        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }
        
        private void HandleSelfFittingAlongAxis(int axis)
        {
            bool match = (axis == 0 ? matchHorizontal : matchVertical);
            if (!match)
                return;
            
            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            drivenRectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(rectTransform, axis));
        }
        
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }
        
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }
        
        protected void SetDirty()
        {
            if (!IsActive())
                return;
            
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
        #endif
    }
}