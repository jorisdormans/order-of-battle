using System;
using System.Collections.Generic;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Shapes;
using System.Diagnostics;

namespace MusketLine.Core
{
    class Raytracer : Component
    {
        private EntityLayer layer;

        public override void OnAdd(Component parent)
        {
            if (!(parent is EntityLayer))
            {
                throw new InvalidOperationException("Raytracer can only be added to an EntityLayer!");
            }

            this.layer = (EntityLayer)parent;

            base.OnAdd(parent);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
        }

        public bool Cast(Vector2 origin, Vector2 target, float maxRange, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition)
        {
            return Cast(origin, target, maxRange, out hitEntity, out hitDistance, out hitPosition, 0, 0, 0);
        }
        public bool Cast(Vector2 origin, Vector2 ray, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition)
        {
            return Cast(origin, ray, out hitEntity, out hitDistance, out hitPosition, 0, 0, 0);
        }
        public bool Cast(Vector2 origin, Vector2 target, float maxRange, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, int skip)
        {
            return Cast(origin, target, maxRange, out hitEntity, out hitDistance, out hitPosition, 0, 0, 0, skip);
        }
        public bool Cast(Vector2 origin, Vector2 ray, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, int skip)
        {
            return Cast(origin, ray, out hitEntity, out hitDistance, out hitPosition, 0, 0, 0, skip);
        }
        public bool Cast(Vector2 origin, Vector2 target, float maxRange, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, uint hasAllFlags, uint hasAnyFlag, uint ignoreFlags)
        {
            return Cast(origin, target, maxRange, out hitEntity, out hitDistance, out hitPosition, hasAllFlags, hasAnyFlag, ignoreFlags, 0);
        }
        public bool Cast(Vector2 origin, Vector2 ray, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, uint hasAllFlags, uint hasAnyFlag, uint ignoreFlags)
        {
            return Cast(origin, ray, out hitEntity, out hitDistance, out hitPosition, hasAllFlags, hasAnyFlag, ignoreFlags, 0);
        }
        public bool Cast(Vector2 origin, Vector2 target, float maxRange, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, uint hasAllFlags, uint hasAnyFlag, uint ignoreFlags, int skip)
        {
            Vector2 ray = (target - origin);
            ray.Normalize();
            ray *= maxRange;

            return Cast(origin, ray, out hitEntity, out hitDistance, out hitPosition, hasAllFlags, hasAnyFlag, ignoreFlags, skip);
        }

        public bool Cast(Vector2 origin, Vector2 ray, out Entity hitEntity, out float hitDistance, out Vector2 hitPosition, uint hasAllFlags, uint hasAnyFlag, uint ignoreFlags, int skip)
        {
            Entity[] hitEntities;
            float[] hitDistances;
            Vector2[] hitPositions;
            if (Cast(origin, ray, hasAllFlags, hasAnyFlag, ignoreFlags, skip, 1, out hitEntities, out hitDistances, out hitPositions))
            {
                hitEntity = hitEntities[0];
                hitDistance = hitDistances[0];
                hitPosition = hitPositions[0];
                return true;
            }
            else
            {
                hitEntity = null;
                hitDistance = 0f;
                hitPosition = Vector2.Zero;
                return false;
            }
        }

        /// <summary>
        /// Cast a ray from origin in direction ray, maximally to the end of ray, returning all objects intersected by the ray, sorted by ray entry point distance.
        /// </summary>
        /// <param name="origin">Start of the ray</param>
        /// <param name="ray">Direction and maximum range of the ray</param>
        /// <param name="hasAllFlags">If set to anything other than 0, ray only includes entities that have at least all these flags set</param>
        /// <param name="hasAnyFlag">If set to anything other than ~0 (uint.MaxValue), ray only includes entities that have at least one of these flags set</param>
        /// <param name="ignoreFlags">Ignores entities that have any of these flags set</param>
        /// <param name="skip">The first number of objects encountered by the ray to skip through</param>
        /// <param name="number">The first number of objects encountered by the ray to return (or all if set to 0)</param>
        /// <param name="hitEntity">Returns the entities hit</param>
        /// <param name="hitDistance">Returns the hit distances of all hit entities</param>
        /// <param name="hitPositions">Returns the hit locations of all hit points</param>
        /// <returns>True if at least <paramref name="number"/> entities were hit by the ray</returns>
        public bool Cast(Vector2 origin, Vector2 ray, uint hasAllFlags, uint hasAnyFlag, uint ignoreFlags, int skip, int number, out Entity[] hitEntities, out float[] hitDistances, out Vector2[] hitPositions)
        {
            Vector2 endPoint = origin + ray;
            Entity[] candidatesA = this.layer.GetEntities();
            int countA = candidatesA.Length;
            Entity[] candidatesB = new Entity[countA];
            int countB = 0;

            int i;
            float currentRadius;

            // Broad phase 1:
            // select only objects within the square area contained within the ray

            float left, right, top, bottom;
            if (origin.X < endPoint.X)
            {
                left = origin.X;
                right = endPoint.X;
            }
            else
            {
                left = endPoint.X;
                right = origin.X;
            }
            if (origin.Y < endPoint.Y)
            {
                top = origin.Y;
                bottom = endPoint.Y;
            }
            else
            {
                top = endPoint.Y;
                bottom = origin.Y;
            }

            for (i = 0; i < countA; i++)
            {
                if ((candidatesA[i].Flags & hasAllFlags) == hasAllFlags
                    && (hasAnyFlag == 0 || (candidatesA[i].Flags & hasAnyFlag) != 0) && (candidatesA[i].Flags & ignoreFlags) == 0)
                {
                    currentRadius = candidatesA[i].Shape.RoughRadius;

                    if (candidatesA[i].Position.X + currentRadius > left && candidatesA[i].Position.X - currentRadius < right
                        && candidatesA[i].Position.Y + currentRadius > top && candidatesA[i].Position.Y - currentRadius < bottom)
                    {
                        candidatesB[countB++] = candidatesA[i];
                    }
                }
            }

            // Broad phase 2:
            // use rough object radius to filter objects around the ray
            // while sorting remaining objects on distance of the bounding sphere front intersection
            // so that we can stop in the narrow phase if an intersection found is closer than the next value

            SortedList<float, Entity> sorted = new SortedList<float, Entity>();
            Dictionary<Entity, Vector2> hitPoint = new Dictionary<Entity, Vector2>();
            Vector2 closestPoint;
            Vector2[] intersections;
            float distance;

            for (i = 0; i < countB; i++)
            {
                closestPoint = PhantomUtils.ClosestPointOnLine(origin, endPoint, candidatesB[i].Position);
                distance = Vector2.Distance(closestPoint, candidatesB[i].Position);
                currentRadius = candidatesB[i].Shape.RoughRadius;

                if (distance < currentRadius)
                {
                    float dist = Vector2.Distance(origin, closestPoint);
                    float newdist = dist - (float)Math.Sqrt(currentRadius * currentRadius - distance * distance);
                    if (sorted.ContainsKey(newdist))
                        sorted[newdist] = candidatesB[i];
                    else
                        sorted.Add(newdist, candidatesB[i]);
                    hitPoint[candidatesB[i]] = origin + (closestPoint - origin) * newdist / dist;
                }
            }

            sorted.Values.CopyTo(candidatesB, 0);
            countB = sorted.Count;

            // Narrow phase:
            // test the objects in order of finding 

            int orderChanged = 0;

            countA = 0;

            for (i = 0; i < countB; i++)
            {
                if (candidatesB[i].Shape is Circle)
                {
                    countA++;
                }
                else
                {
                    sorted.RemoveAt(sorted.Values.IndexOf(candidatesB[i]));

                    intersections = candidatesB[i].Shape.IntersectEdgesWithLine(origin, endPoint);

                    if (intersections.Length > 0)
                    {
                        distance = Vector2.Distance(origin, intersections[0]);
                        closestPoint = intersections[0];
                        for (int n = 0; n < intersections.Length; n++)
                        {
                            if (Vector2.Distance(origin, intersections[n]) < distance)
                            {
                                distance = Vector2.Distance(origin, intersections[n]);
                                closestPoint = intersections[n];
                            }
                        }

                        try
                        {
                            sorted.Add(distance, candidatesB[i]);
                            hitPoint[candidatesB[i]] = closestPoint;

                            if (i < countB - 1 && sorted.Values.IndexOf(candidatesB[i + 1]) < sorted.Values.IndexOf(candidatesB[i]))
                            {
                                orderChanged++;
                            }
                            else
                            {
                                countA++;
                                if (orderChanged > 0) countA += orderChanged;
                            }
                        }
                        catch { }
                    }
                }
                if (countA == number + skip)
                    break;
            }

            if (countA > number + skip) countA = number + skip;

            countA = Math.Max(0, countA - skip);

            hitEntities = new Entity[countA];
            hitDistances = new float[countA];
            hitPositions = new Vector2[countA];

            for (i = 0; i < Math.Min(countA, sorted.Count); i++)
            {
                hitEntities[i] = sorted.Values[i + skip];
                hitDistances[i] = sorted.Keys[i + skip];
                hitPositions[i] = hitPoint[hitEntities[i]];
            }

            return Math.Min(countA, sorted.Count) > 0;
        }
    }
}
