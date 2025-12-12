using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.Test.Tests
{
    public class MoveTowardsLineTests
    {
        private class Point3D
        {
            public double X;
            public double Y;
            public double Z;
        }

        private static class Utility
        {
            private const double M_PI = Math.PI;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="P1">Line point 1.</param>
            /// <param name="P2">Line point 2.</param>
            /// <param name="player">Player coordinate.</param>
            /// <param name="player_angle">Angle player is facing. Starts at 0, which is to the right along x axis.</param>
            /// <param name="distance">Distance from player to line.</param>
            /// <param name="turn_angle">Angle player needs to turn to face line. Add to player angle. Angles are counter clockwise around origin.</param>
            public static void MoveTowardsLine(Point3D P1, Point3D P2, Point3D player,
                             double player_angle, out double distance, out double turn_angle)
            {
                // Line direction vector
                double dx = P2.X - P1.X;
                double dy = P2.Y - P1.Y;

                double denom = (dx * dx + dy * dy);
                if (denom == 0)
                {
                    distance = 0;
                    turn_angle = 0;
                    return;
                }

                // Distance from player to line
                distance = Math.Abs(dy * player.X - dx * player.Y + P2.X * P1.Y - P2.Y * P1.X) /
                            Math.Sqrt(denom);

                if (Math.Abs(distance) < 0.0001)
                {
                    distance = 0;
                    turn_angle = 0;
                    return;
                }

                // Find projection of player onto line
                double t = ((player.X - P1.X) * dx + (player.Y - P1.Y) * dy) / denom;
                double projX = P1.X + t * dx;
                double projY = P1.Y + t * dy;

                // Vector from player to closest point on line
                double vecX = projX - player.X;
                double vecY = projY - player.Y;

                // Angle player needs to face
                double target_angle = Math.Atan2(vecY, vecX);

                // Turn angle = difference between target and current facing angle
                double diff = target_angle - player_angle;

                // Normalize to [-pi, pi]
                while (diff > M_PI) diff -= 2 * M_PI;
                while (diff < -M_PI) diff += 2 * M_PI;

                turn_angle = diff;
            }
        }

        public class PlayerPointTests
        {
            private const double M_PI = Math.PI;

            [Theory]
            [InlineData(0, 0, 0, 0, 0, 0)]
            [InlineData(0, 0, 4, 0, 2, 0)]
            [InlineData(0, 0, 0, -4, 0, -2)]
            [InlineData(0, 0, -4, 0, -2, 0)]
            [InlineData(0, 0, 0, 4, 0, 2)]
            public void ZeroDistanceTests(
                double p1x,
                double p1y,
                double p2x,
                double p2y,
                double p3x,
                double p3y
                )
            {
                var p1 = new Point3D() { X = p1x, Y = p1y, Z = 0 };
                var p2 = new Point3D() { X = p2x, Y = p2y, Z = 0 };

                var player = new Point3D() { X = p3x, Y = p3y, Z = 0 };

                // angle 0 is to the right, along the x axis.
                // angle 90 is straight up.
                double start_angle = 180;
                double player_angle = (start_angle * M_PI) / 180.0;

                double distance;
                double turn_angle;
                double turn_angle_deg;

                Utility.MoveTowardsLine(p1, p2, player, player_angle, out distance, out turn_angle);

                turn_angle_deg = turn_angle * 180.0 / M_PI;
                double final_angle = start_angle + turn_angle_deg;

                Assert.Equal(0, distance);
                Assert.Equal(0, turn_angle);
            }

            [Theory]
            [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0)]

            [InlineData(0, 0, 4, 4, 3, 1, 0, 1.414, 135)]
            [InlineData(0, 0, 4, 4, 3, 1, 90, 1.414, 45)]
            [InlineData(0, 0, 4, 4, 3, 1, 135, 1.414, 0)]
            [InlineData(0, 0, 4, 4, 3, 1, 180, 1.414, -45)]
            [InlineData(0, 0, 4, 4, 3, 1, 270, 1.414, -135)]
            [InlineData(0, 0, 4, 4, 3, 1, -90, 1.414, -135)]

            [InlineData(0, 0, 4, 4, 1, 3, 0, 1.414, -45)]
            [InlineData(0, 0, 4, 4, 1, 3, 90, 1.414, -135)]
            [InlineData(0, 0, 4, 4, 1, 3, 135, 1.414, -180)]
            [InlineData(0, 0, 4, 4, 1, 3, 180, 1.414, 135)]
            [InlineData(0, 0, 4, 4, 1, 3, 270, 1.414, 45)]
            [InlineData(0, 0, 4, 4, 1, 3, -90, 1.414, 45)]
            public void AngleTests(
                double p1x,
                double p1y,
                double p2x,
                double p2y,
                double p3x,
                double p3y,
                double start_angle, // degrees

                double expected_distance,
                double expected_angle
                )
            {
                var p1 = new Point3D() { X = p1x, Y = p1y, Z = 0 };
                var p2 = new Point3D() { X = p2x, Y = p2y, Z = 0 };

                var player = new Point3D() { X = p3x, Y = p3y, Z = 0 };

                // angle 0 is to the right, along the x axis.
                // angle 90 is straight up.
                double player_angle = (start_angle * M_PI) / 180.0;

                double distance;
                double turn_angle;
                double turn_angle_deg;

                Utility.MoveTowardsLine(p1, p2, player, player_angle, out distance, out turn_angle);

                turn_angle_deg = turn_angle * 180.0 / M_PI;
                double final_angle = start_angle + turn_angle_deg;

                double delta;

                delta = Math.Abs(expected_distance - distance);
                Assert.True(delta < 0.001);

                delta = Math.Abs(expected_angle - turn_angle_deg);
                Assert.True(delta < 0.001);
            }

            public class Startup
            {
            }
        }
    }
}
