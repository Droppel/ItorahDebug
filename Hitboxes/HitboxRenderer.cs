using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ItorahDebug.Hitbox {
    public class HitboxRender : MonoBehaviour {

        private readonly HashSet<Collider2D> colliders = new HashSet<Collider2D>();

        //public static float LineWidth => Math.Max(0.7f, Screen.width / 960f * GameCameras.instance.tk2dCam.ZoomFactor);
        public static float LineWidth => Math.Max(0.7f, Screen.width / 960f * 0.5f);

        private void Start() {
            foreach (Collider2D col in Resources.FindObjectsOfTypeAll<Collider2D>()) {
                TryAddHitboxes(col);
            }
        }

        public void UpdateHitbox(GameObject go) {
            foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>(true)) {
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

            if (collider2D is BoxCollider2D || collider2D is PolygonCollider2D || collider2D is EdgeCollider2D || collider2D is CircleCollider2D) {
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
                case BoxCollider2D boxCollider2D:
                    Color color = Color.red;
                    if (boxCollider2D.gameObject.name.Contains("RespawnPoint")) {
                        color = Color.magenta;
                    }
                    Vector2 halfSize = boxCollider2D.size / 2f;
                    Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y);
                    Vector2 topRight = halfSize;
                    Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y);
                    Vector2 bottomLeft = -halfSize;
                    List<Vector2> boxPoints = new List<Vector2>
                    {
                        topLeft, topRight, bottomRight, bottomLeft, topLeft
                    };
                    DrawPointSequence(boxPoints, camera, collider2D, lineWidth, color);
                    break;
                case EdgeCollider2D edgeCollider2D:
                    DrawPointSequence(new List<Vector2>(edgeCollider2D.points), camera, collider2D, lineWidth, Color.yellow);
                    break;
                case PolygonCollider2D polygonCollider2D:
                    for (int i = 0; i < polygonCollider2D.pathCount; i++) {
                        List<Vector2> polygonPoints = new List<Vector2>(polygonCollider2D.GetPath(i));
                        if (polygonPoints.Count > 0) {
                            polygonPoints.Add(polygonPoints[0]);
                        }
                        DrawPointSequence(polygonPoints, camera, collider2D, lineWidth, Color.green);
                    }
                    break;
                case CircleCollider2D circleCollider2D:
                    Vector2 center = LocalToScreenPoint(camera, collider2D, Vector2.zero);
                    Vector2 right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
                    int radius = (int)Math.Round(Vector2.Distance(center, right));
                    Drawing.DrawCircle(center, radius, Color.red, lineWidth, true, Mathf.Clamp(radius / 16, 4, 32));
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