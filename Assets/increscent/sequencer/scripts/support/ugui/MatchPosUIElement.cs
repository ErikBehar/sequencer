using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class MatchPosUIElement : UIBehaviour, ILayoutSelfController
    {
        public float horizontalOffset = 50;
        public float verticalOffset = 50;

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
        
        
        public virtual void SetLayoutHorizontal()
        {
            drivenRectTransform.position = m_Rect.position + Vector3.up * verticalOffset + Vector3.right * horizontalOffset;
        }
        
        public virtual void SetLayoutVertical()
        {
            drivenRectTransform.position = m_Rect.position + Vector3.up * verticalOffset + Vector3.right * horizontalOffset;
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