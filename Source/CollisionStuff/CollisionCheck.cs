namespace Asteroids2.Source.CollisionStuff;

internal static class CollisionCheck
{
    /// <summary>
    /// Check for collision between two circles
    /// This also checks for tunneling, so that if the circles are going to pass between each other
    /// </summary>
    /// <param name="a">teh first circle</param>
    /// <param name="b">the second circle</param>
    /// <returns>These circles are either colliding, or going to collide</returns>
    public static bool CircleCircleCollision(Circle a, Circle b)
    {
        // calculate relative velocity and position
        float dvx = b.Pos.X - b.OldPos.X - (a.Pos.X - a.OldPos.X);
        float dvy = b.Pos.Y - b.OldPos.Y - (a.Pos.Y - a.OldPos.Y);
        float dx = b.OldPos.X - a.OldPos.X;
        float dy = b.OldPos.Y - a.OldPos.Y;

        // check if circles are already colliding
        float sqRadiiSum = a.Radius + b.Radius;
        sqRadiiSum *= sqRadiiSum;
        float pp = dx * dx + dy * dy - sqRadiiSum;

        if (pp < 0) return true;

        // check if the circles are moving away from each other and hence can’t collide
        float pv = dx * dvx + dy * dvy;

        if (pv >= 0) return false;

        // check if the circles can reach each other between the frames
        float vv = dvx * dvx + dvy * dvy;

        if (((pv + vv) <= 0) && ((vv + 2 * pv + pp) >= 0)) return false;

        // if we've gotten this far then it’s possible for intersection if the distance between
        // the circles is less than the radii sum when it’s at a minimum. Therefore find the time
        // when the distance is at a minimum and test this
        float tmin = -pv / vv;

        return (pp + pv * tmin) < 0;
    }
}