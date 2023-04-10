using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotVTT
{
    /// <summary>
    /// Collection of Solid and Void/Hole polygons.
    /// </summary>
    public class CompositePolygon
    {
        public List<Vector2[]> Solids { get; set; } = new();
        public List<Vector2[]> Voids  { get;set; } = new();


        /// <summary>
        /// Combines the regions with a new polygon.
        /// </summary>
        /// <param name="newPoly">The new polygon to combine. Is considered a "void" with clockwize winding. </param>
        public void CombineWith(Vector2[] newPoly)
        {
            UnionWith(new[] { newPoly });
        }

        /// <summary>
        /// Unions a polygon with this set.
        /// </summary>
        /// <param name="solid">The polygon to union. Is treated as a solid regardless of winding.</param>
        public void Add(Vector2[] solid)
        {
            if (Geometry2D.IsPolygonClockwise(solid))
            {
                CombineWith(solid.Reverse().ToArray());
            }
            else
            {
                CombineWith(solid);
            }
        }

        /// <summary>
        /// Subtracts a polygon with this set.
        /// </summary>
        /// <param name="hole">The polygon to subtract. Is treated as a hole regardless of winding.</param>
        public void Subtract(Vector2[] hole)
        {
            if (Geometry2D.IsPolygonClockwise(hole))
            {
                CombineWith(hole);
            }
            else
            {
                CombineWith(hole.Reverse().ToArray());
            }
        }

        /// <summary>
        /// Combines (in-place) another composite polygon with this set.
        /// </summary>
        /// <param name="left">The <see cref="CompositePolygon"/> to merge with this one.</param>
        public void UnionWith(CompositePolygon left)
        {
            var incomming = left.Solids.Select(MakeSolid).Concat(left.Voids.Select(MakeHole));
            UnionWith(incomming);
        }

        /// <summary>
        /// Merges polygons with this set.
        /// </summary>
        /// <param name="incomming">The new polygons to combine. A polygon is considered a "void" with clockwize winding.</param>
        public void UnionWith(IEnumerable<Vector2[]> incomming)
        {
            var newSolids = new List<Vector2[]>();
            var newVoids = new List<Vector2[]>();
            var solids = new Queue<Vector2[]>(Solids);
            var voids = new Queue<Vector2[]>(Voids);

            var incommingQueue = new Queue<Vector2[]>();
            foreach ( var newPoly in incomming )
            {
                incommingQueue.Enqueue(newPoly);
            }

            while (incommingQueue.TryDequeue(out var currentPoly))
            {
                // clockwise = hole
                bool isHole = Geometry2D.IsPolygonClockwise(currentPoly);
                var currentSolids = solids;
                var currentVoids = voids;
                solids = new Queue<Vector2[]>();
                voids = new Queue<Vector2[]>();

                while (currentPoly != null && currentSolids.TryDequeue(out var testSolid))
                {
                    var merged = isHole
                        ? Geometry2D.ClipPolygons(testSolid, currentPoly)
                        : Geometry2D.MergePolygons(testSolid, currentPoly);

                    if (merged.Count == 1)
                    {
                        incommingQueue.Enqueue(merged[0]);
                        currentPoly = null;
                        continue;
                    }

                    bool changed = true;
                    foreach (var poly in merged)
                    {
                        if (!poly.Except(currentPoly).Any())
                        {
                            changed = false;
                            continue;
                        }

                        if (poly.Except(testSolid).Any() && incommingQueue.Any())
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                voids.Enqueue(poly);
                            }
                            else
                            {
                                solids.Enqueue(poly);
                            }
                        }
                        else
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                newVoids.Add(poly);
                            }
                            else
                            {
                                newSolids.Add(poly);
                            }
                        }
                    }
                    if (changed)
                    {
                        currentPoly = null;
                    }
                }
                while (currentPoly != null && currentVoids.TryDequeue(out var testVoid))
                {
                    var merged = isHole
                        ? Geometry2D.ClipPolygons(testVoid, currentPoly)
                        : Geometry2D.MergePolygons(testVoid, currentPoly);

                    if (merged.Count == 1)
                    {
                        incommingQueue.Enqueue(merged[0]);
                        currentPoly = null;
                        continue;
                    }

                    bool changed = true;
                    foreach (var poly in merged)
                    {
                        if (!poly.Except(currentPoly).Any())
                        {
                            changed = true;
                            continue;
                        }

                        if (poly.Except(testVoid).Any() && incommingQueue.Any())
                        {
                            if (Geometry2D.IsPolygonClockwise(poly))
                            {
                                voids.Enqueue(poly);
                            }
                            else
                            {
                                solids.Enqueue(poly);
                            }
                        }
                        else
                        {
                            newVoids.Add(poly);
                        }
                    }

                    if (changed)
                    {
                        currentPoly = null;
                    }
                }

                if (currentPoly != null)
                {
                    if (Geometry2D.IsPolygonClockwise(currentPoly))
                    {
                        newVoids.Add(currentPoly);
                    }
                    else
                    {
                        newSolids.Add(currentPoly);
                    }
                }
            }

            Solids = newSolids;
            Voids = newVoids;
        }

        private static void EnsureSolid(ref Vector2[] polygon)
        {
            if (Geometry2D.IsPolygonClockwise(polygon))
            {
                Array.Reverse(polygon);
            }
        }

        private static void EnsureHole(ref Vector2[] polygon)
        {
            if (!Geometry2D.IsPolygonClockwise(polygon))
            {
                Array.Reverse(polygon);
            }
        }

        private static Vector2[] MakeSolid(Vector2[] polygon)
        {
            return Geometry2D.IsPolygonClockwise(polygon)
                ? polygon.Reverse().ToArray()
                : polygon;
        }



        private static Vector2[] MakeHole(Vector2[] polygon)
        {
            return Geometry2D.IsPolygonClockwise(polygon)
                ? polygon
                : polygon.Reverse().ToArray();
        }
    }
}
