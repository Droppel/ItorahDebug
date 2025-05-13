using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ItorahDebug.Hitbox {
    public class HitboxTerrainRender : MonoBehaviour {

        private readonly HashSet<Collider2D> colliders = new HashSet<Collider2D>();

        //public static float LineWidth => Math.Max(0.7f, Screen.width / 960f * GameCameras.instance.tk2dCam.ZoomFactor);
        public static float LineWidth => Math.Max(0.7f, Screen.width / 960f * 0.5f);

        private void Start() {
            foreach (Collider2D col in Resources.FindObjectsOfTypeAll<Collider2D>()) {
                TryAddHitboxes(col);
            }
        }

        public void UpdateHitbox(GameObject go) {
            foreach (Collider2D col in go.GetComponentsInChildren<CompositeCollider2D>(true)) {
                TryAddHitboxes(col);
            }
        }

        private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point) {
            Vector2 result = camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
            return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
        }

        private void TryAddHitboxes(Collider2D collider2D) {
            if (collider2D == null) {
                return;
            }

            if (collider2D is CompositeCollider2D) {
                if (colliders.Contains(collider2D)) {
                    return;
                }
            }
            colliders.Add(collider2D);
        }

        private void OnGUI() {
            if (Event.current?.type != EventType.Repaint || Camera.main == null) {
                return;
            }

            GUI.depth = int.MaxValue;
            Camera camera = Camera.main;
            float lineWidth = LineWidth;
            foreach (Collider2D collider2D in colliders) {
                DrawHitbox(camera, collider2D, lineWidth);
            }
        }

        private void DrawHitbox(Camera camera, Collider2D collider2D, float lineWidth) {
            if (collider2D == null || !collider2D.isActiveAndEnabled) {
                return;
            }

            int origDepth = GUI.depth;
            switch (collider2D) {
                case CompositeCollider2D compositeCollider2D:
                    for (int i = 0; i < compositeCollider2D.pathCount; i++) {
                        List<Vector2> compositePoints = new List<Vector2>();
                        compositeCollider2D.GetPath(i, compositePoints);
                        if (compositePoints.Count > 0) {
                            compositePoints.Add(compositePoints[0]);
                        }
                        DrawPointSequence(compositePoints, camera, collider2D, lineWidth, Color.blue);
                    }
                    break;
            }

             GUI.depth = origDepth;
        }

        private void DrawPointSequence(List<Vector2> points, Camera camera, Collider2D collider2D, float lineWidth, Color color) {
            for (int i = 0; i < points.Count - 1; i++) {
                Vector2 pointA = LocalToScreenPoint(camera, collider2D, points[i]);
                Vector2 pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);
                Drawing.DrawLine(pointA, pointB, color, lineWidth, true);
            }
        }
    }
}