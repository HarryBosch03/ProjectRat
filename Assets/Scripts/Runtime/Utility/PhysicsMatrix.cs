using UnityEngine;

namespace Runtime.Utility
{
    public static class PhysicsMatrix
    {
        public static int[] matrixByLayer;
        
        public static int GetCollisionMaskForLayer(int layer)
        {
            if (matrixByLayer == null)
            {
                matrixByLayer = new int[32];
                for (var i = 0; i < 32; i++)
                {
                    var mask = 0;
                    
                    for (var j = 0; j < 32; j++)
                    {
                        if (!Physics.GetIgnoreLayerCollision(i, j))
                        {
                            mask |= 1 << j;
                        }
                    }

                    matrixByLayer[i] = mask;
                }
            }

            return matrixByLayer[layer];
        }
    }
}