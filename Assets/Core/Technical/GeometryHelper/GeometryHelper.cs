/// GeometryHelper in #PROJECTNAME#
/// --- 	SUMMARY : 	---
///
/// --- 	NOTES : 	---
///
using System;
using UnityEngine;

public static class GeometryHelper
{
    #region Methods

    /// <summary>
    /// Is a set of three points ordered in clockwise or counterclockwise order?
    /// </summary>
    /// <param name="_firstPoint">The first point of the triangle</param>
    /// <param name="_secondPoint">The second point of the triangle</param>
    /// <param name="_thirdPoint">The Third point of the triangle</param>
    /// <param name="_triangleNormal">The normal of the triangle</param>
    /// <returns>return true if the triangle is ordered clockwise acording to the normal</returns>
    public static bool IsClockwiseTriangle(Vector3 _firstPoint, Vector3 _secondPoint, Vector3 _thirdPoint, Vector3 _triangleNormal)
    {
        return Mathf.Sign(Vector3.Dot(Vector3.Cross((_secondPoint - _firstPoint).normalized, (_thirdPoint - _firstPoint).normalized), _triangleNormal)) > 0;
    }
    #endregion

}
