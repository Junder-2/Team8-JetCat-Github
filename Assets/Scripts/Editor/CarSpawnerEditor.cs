using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(CarSpawn))]
    public class CarSpawnerEditor : UnityEditor.Editor
    {
        private CarSpawn _carSpawn;

        private void OnEnable()
        {
            _carSpawn = (CarSpawn)target;

            _lastTransform = _carSpawn.transform.position;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if(!_carSpawn.UseEditorOverride())
                return;
            
            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(_carSpawn, "Add Point");
                _carSpawn.localPoints.Insert( (_carSpawn.localPoints.Count-1)/2 ,_lastTransform + Vector3.up);
            }

            EditorGUI.EndChangeCheck();
        }

        private Vector3 _lastTransform;

        private void OnSceneGUI()
        {
            bool directionRight;
            
            var infoLabel = new GUIStyle
            {
                fontSize = Mathf.FloorToInt(30),
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.red,
                    background = Texture2D.whiteTexture
                }
            };

            if (!_carSpawn.UseEditorOverride())
            {
                Vector3[] pos = _carSpawn.GetDefaultPositions();
                
                directionRight = _carSpawn.GetDriveDirection() >= 0;

                for (int i = 0; i < pos.Length; i++)
                {
                    int index = directionRight ? i : pos.Length -1 - i;
                    
                    Handles.Label(pos[index], i.ToString(), infoLabel);
                }
                
                return;
            }

            if(_carSpawn.localPoints.Count == 0)
                _carSpawn.SetDefaultPositions();

            Vector3 transformDifference = _carSpawn.transform.position - _lastTransform;

            EditorGUI.BeginChangeCheck();

            Vector3[] points = _carSpawn.localPoints.ToArray();

            directionRight = _carSpawn.GetDriveDirection() >= 0;

            for (int i = 0; i < points.Length; i++)
            {
                int index = directionRight ? i : points.Length -1 - i;

                points[index] += transformDifference;
                
                points[index] = Handles.PositionHandle(points[index], Quaternion.identity);
                
                Handles.Label(points[index], i.ToString(), infoLabel);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_carSpawn, "MovePosition");
                _carSpawn.localPoints = new List<Vector3>(points);
            }
            else if(transformDifference != Vector3.zero)
                _carSpawn.localPoints = new List<Vector3>(points);

            _lastTransform = _carSpawn.transform.position;
        }
    }
}