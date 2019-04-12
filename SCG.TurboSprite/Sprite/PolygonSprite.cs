#region copyright
/*
* Copyright (c) 2018, Dave Voorhis
* Based on PolygonSprite by Dion Kurczek
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the <organization> nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY DAVE VOORHIS ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL DAVE VOORHIS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace SCG.TurboSprite
{
    public class PolygonSprite : Sprite
    {
        private int _lastFrame = 0;
        private int _lastAngle;
        private readonly PointF[][] _rotatedPoints;

        // Construct a polygon-based sprite with 1 or more arrays of points that can be selected to create animation.
        public PolygonSprite(PointF[][] model)
        {
            Model = model;
            _lastAngle = -1;
            _lastFrame = 0;
            FrameCount = model.Length;
            _rotatedPoints = new PointF[FrameCount][];
            for (int i = 0; i < FrameCount; i++)
            {
                int polySize = model[i].Length;
                _rotatedPoints[i] = new PointF[polySize];
            }
            RotateAndAnimate();
        }

        public PointF[][] Model { get; internal set; }

        // Construct a non-animated polygon-based Sprite.
        public PolygonSprite(PointF[] points) : this(new PointF[][] { points }) {}

        // Select the specific Points collection to display.
        public int Frame { get; set; }

        // Get the number of animation frames.
        public int FrameCount { get; internal set; }

        // Switch to next frame.
        public void IncrementFrame()
        {
            int nextFrame = Frame + 1;
            if (nextFrame >= FrameCount)
            {
                nextFrame = 0;
            }
            Frame = nextFrame;
        }

        private void ObtainShape()
        {
            // Set the shape of the sprite based on largest dimension from center
            float x1 = 0;
            float y1 = 0;
            float x2 = 0;
            float y2 = 0;
            _rotatedPoints[_lastFrame].ToList().ForEach(point =>
            {
                if (point.X < x1)
                {
                    x1 = point.X;
                }
                if (point.X > x2)
                {
                    x2 = point.X;
                }
                if (point.Y < y1)
                {
                    y1 = point.Y;
                }
                if (point.Y > y2)
                {
                    y2 = point.Y;
                }
            });
            Shape = new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }

        // Access line color
        public Color Color { get; set; } = Color.Red;

        // Access line width
        public int LineWidth { get; set; }

        // Determine whether the Sprite is filled
        public bool IsFilled { get; set; }

        // Access the fill color
        public Color FillColor { get; set; } = Color.Empty;

        private void RotateAndAnimate()
        {
            // Process rotation and animation
            if (FacingAngle != _lastAngle || Frame != _lastFrame)
            {
                _lastAngle = FacingAngle;
                _lastFrame = Frame;
                float sin = Sprite.Sin(FacingAngle);
                float cos = Sprite.Cos(FacingAngle);
                for (int point = 0; point < _rotatedPoints[_lastFrame].Length; point++)
                {
                    _rotatedPoints[_lastFrame][point].X = Model[_lastFrame][point].X * cos - Model[_lastFrame][point].Y * sin;
                    _rotatedPoints[_lastFrame][point].Y = Model[_lastFrame][point].Y * cos + Model[_lastFrame][point].X * sin;
                }
                ObtainShape();
            }
        }

        // Render the sprite - draw the polygon
        protected internal override void Render(Graphics graphics)
        {
            RotateAndAnimate();

            graphics.TranslateTransform(X - Surface.OffsetX, Y - Surface.OffsetY);

            // Fill it?
            if (IsFilled)
            {
                using (Brush brush = new SolidBrush(FillColor))
                {
                    graphics.FillPolygon(brush, _rotatedPoints[_lastFrame]);
                }
            }

            // Draw outline
            using (Pen pen = new Pen(Color, LineWidth))
            {
                graphics.DrawPolygon(pen, _rotatedPoints[_lastFrame]);
            }

            graphics.TranslateTransform(-(X + Surface.OffsetX), -(Y - Surface.OffsetY));
        }
    }
}
