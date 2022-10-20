using UnityEngine;

namespace Car
{
    public struct WayPoint
    {
        public Vector3 Position;
    }
    
    public static class CarHelpers
    {
        public static WayPoint[] GenerateStraightPath(Vector3 start, Vector3 target, float noiseStrength, int points)
        {
            WayPoint[] wayPoints = new WayPoint[points];

            for (int i = 0; i < points; i++)
            {
                float posInPath = (float)i/(points-1);

                Vector3 pos = Vector3.Lerp(start, target, posInPath);

                pos.z += Random.Range(-1f, 1f) * noiseStrength;

                wayPoints[i].Position = pos;
            }
            
            return wayPoints;
        }

        /*public static WayPoint[] GenerateFromPositionsWithNoise(Vector3[] predefinedPoints, float noiseStrength, int points) //does not properly work
        {
            points = Mathf.Max(predefinedPoints.Length+1, points);
            
            int pointsPerSegment = points /predefinedPoints.Length;
            
            WayPoint[] wayPoints = new WayPoint[points];

            int indexInSegment = 0;
            int segment = 1;

            for (int i = 0; i < points; i++)
            {
                float posInPath = (float)indexInSegment/(pointsPerSegment*segment-1);
                Debug.Log(segment);

                Vector3 pos = Vector3.Lerp(predefinedPoints[segment-1], predefinedPoints[segment], posInPath);

                pos.z += Random.Range(-1f, 1f) * noiseStrength;

                wayPoints[i].Position = pos;

                indexInSegment++;
                if (indexInSegment < pointsPerSegment || segment >= predefinedPoints.Length - 1) continue;
                
                indexInSegment -= pointsPerSegment;
                segment++;
            }
            
            return wayPoints;
        }*/

        public static WayPoint[] GenerateFromPositions(Vector3[] predefinedPoints)
        {
            WayPoint[] wayPoints = new WayPoint[predefinedPoints.Length];

            for (int i = 0; i < wayPoints.Length; i++)
            {
                wayPoints[i].Position = predefinedPoints[i];
            }

            return wayPoints;
        }
    }
}
