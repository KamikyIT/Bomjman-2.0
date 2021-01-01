using System.Collections.Generic;
using System.Linq;
using TbsFramework.Cells;
using UnityEngine;

namespace TbsFramework.Example2
{
    public class MyOtherHexagon : Hexagon
    {
        const string Highlighter = @"Highlighter";

        public GroundType GroundType;
        public bool IsSkyTaken;//Indicates if a flying unit is occupying the cell.

        private Vector3 dimensions = new Vector3(5.3f, 4.6f, 0f);

        static Color ReachableColor = new Color(1, 0.92f, 0.016f, 1);
        static Color PathColor = new Color(0, 1, 0, 1);
        static Color HighlitedColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        static Color UnMarkColor = new Color(1, 1, 1, 0);

        SpriteRenderer _highlighterSpriteRenderer;
        public SpriteRenderer HighlighterSpriteRenderer {
            get {
                if (_highlighterSpriteRenderer == null)
                {
                    var go = transform.Find(Highlighter);
                    if (go != null)
                        _highlighterSpriteRenderer = go.GetComponent<SpriteRenderer>();
                }
                return _highlighterSpriteRenderer;
            }
        }

        List<SpriteRenderer> _childSpriteRenderers;
        public List<SpriteRenderer> ChildSpriteRenderers {
            get {
                if (_childSpriteRenderers == null)
                {
                    var go = transform.Find(Highlighter);
                    _childSpriteRenderers = new List<SpriteRenderer>();
                    foreach (Transform child in go.transform)
                    {
                        var sprite = child.GetComponent<SpriteRenderer>();
                        if (sprite != null)
                            _childSpriteRenderers.Add(sprite);
                    }
                }
                return _childSpriteRenderers;
            }
        }

        public void Start()
        {
            SetColor(UnMarkColor);
        }

        public override void MarkAsReachable()
        {
            SetColor(ReachableColor);
        }
        public override void MarkAsPath()
        {
            SetColor(PathColor);
        }
        public override void MarkAsHighlighted()
        {
            SetColor(HighlitedColor);
        }
        public override void UnMark()
        {
            SetColor(UnMarkColor);
        }

        private void SetColor(Color color)
        {
            if (HighlighterSpriteRenderer != null)
                HighlighterSpriteRenderer.color = color;

            if (ChildSpriteRenderers != null)
            {
                var visibleChildColor = new Color(color.r, color.g, color.b, 1);
                ChildSpriteRenderers.ForEach(x => x.color = visibleChildColor);
            }
        }

        public override Vector3 GetCellDimensions()
        {
            return dimensions;
        }
    }

    public enum GroundType
    {
        Land,
        Water
    };
}