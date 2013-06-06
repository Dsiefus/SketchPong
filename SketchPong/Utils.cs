using Microsoft.Xna.Framework;
using System.Timers;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace sketchPong
{
    static class Utils
    {
        public static BoundingBox GetBBFromPosAndRec(Vector2 pos, Rectangle rec)
        {
            BoundingBox bbRet = new BoundingBox();
            bbRet.Min.X = pos.X + rec.X;
            bbRet.Min.Y = pos.Y + rec.Y;
            bbRet.Max.X = pos.X + rec.X + rec.Width;
            bbRet.Max.Y = pos.Y + rec.Y + rec.Height;

            return bbRet;
        }

        public static Rectangle GetRecFromBoundingBox(BoundingBox bb)
        {
            Rectangle rec = new Rectangle();
            rec.X = (int)bb.Min.X;
            rec.Y = (int)bb.Min.Y;
            rec.Width = (int)bb.Max.X - (int)bb.Min.X;
            rec.Height = (int)bb.Max.Y - (int)bb.Min.Y;
            return rec;
        }

        public static bool Intersects(Rectangle a, Rectangle b)
        {
            // check if two Rectangles intersect
            return (a.Right > b.Left && a.Left < b.Right &&
                    a.Bottom > b.Top && a.Top < b.Bottom);
        }

        // Test Pixel Perfect Collision between two objects
        public static bool TestPPCollision(Texture2D texA, Texture2D texB, BoundingBox boundsA, BoundingBox boundsB)
        {
            bool collision = false;

            if (boundsA.Intersects(boundsB))
            {
                int texPixelDimensionsA = texA.Width * texA.Height;
                Color[] pixelsA = new Color[texPixelDimensionsA];
                texA.GetData<Color>(0, null, pixelsA, 0, texPixelDimensionsA);

                int texPixelDimensionsB = texB.Width * texB.Height;
                Color[] pixelsB = new Color[texPixelDimensionsB];
                texB.GetData<Color>(0, null, pixelsB, 0, texPixelDimensionsB);

                int maxX = (int)Math.Max(boundsA.Min.X, boundsB.Min.X);
                int minX = (int)Math.Min(boundsA.Max.X, boundsB.Max.X);

                int maxY = (int)Math.Max(boundsA.Min.Y, boundsB.Min.Y);
                int minY = (int)Math.Min(boundsA.Max.Y, boundsB.Max.Y);

                for (int y = maxY; y < minY; ++y)
                {
                    for (int x = maxX; x < minX; ++x)
                    {
                        if ((pixelsA[(x - (int)boundsA.Min.X) + (y - (int)boundsA.Min.Y) * texA.Width] != Color.Transparent) &&
                            (pixelsB[(x - (int)boundsB.Min.X) + (y - (int)boundsB.Min.Y) * texB.Width] != Color.Transparent))
                        {
                            collision = true;
                            x = minX;
                            y = minY;
                        }
                    }
                }
            }
            return collision;
        }
    }
}
