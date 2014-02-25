﻿using System;
using UnityEngine;

namespace Mercraft.Models.Algorithms
{
    /// <summary>
    /// Provides the methods to convert geo coordinates to map coordinates and vice versa
    /// </summary>
    public static class GeoProjection
    {
        /// <summary>
        /// The circumference at the equator (latitude 0)
        /// </summary>
        const int LatitudeEquator = 40075160;

        /// <summary>
        /// distance of full circle around the earth through the poles
        /// </summary>
        const int CircleDistance = 40008000;

        /// <summary>
        /// Calculates map coordinate from geo coordinate
        /// see http://stackoverflow.com/questions/3024404/transform-longitude-latitude-into-meters?rq=1
        /// </summary>
        public static Vector2 ToMapCoordinate(GeoCoordinate relativeNullPoint, GeoCoordinate coordinate)
        {
            double deltaLatitude = coordinate.Latitude - relativeNullPoint.Latitude;
            double deltaLongitude = coordinate.Longitude - relativeNullPoint.Longitude;
            double latitudeCircumference = LatitudeEquator * Math.Cos(MathUtility.Deg2Rad(relativeNullPoint.Latitude));
            double resultX = deltaLongitude * latitudeCircumference / 360;
            double resultY = deltaLatitude * CircleDistance / 360;
            
            return new Vector2((float)resultX, (float) resultY);
        }

        /// <summary>
        /// Calculates geo coordinate from map coordinate. Reverse operation to ToMapCoordinates()
        /// </summary>
        public static GeoCoordinate ToGeoCoordinate(GeoCoordinate relativeNullPoint, Vector2 mapPoint)
        {
            double latitudeCircumference = LatitudeEquator * Math.Cos(MathUtility.Deg2Rad(relativeNullPoint.Latitude));

            var deltaLongitude = (mapPoint.x * 360) / latitudeCircumference;
            var deltaLatitude = (mapPoint.y * 360) / CircleDistance;

            return new GeoCoordinate(
                relativeNullPoint.Latitude + deltaLatitude, 
                relativeNullPoint.Longitude + deltaLongitude);
        }
    }
}
