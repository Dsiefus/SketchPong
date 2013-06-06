using System;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace sketchPong
{
    public class Ball
    {
        private Vector2 position;       

        private const float MaxSpeed = 1000f;
        private Vector2 speed;

        private Texture2D texture;
        private BoundingSphere bounds;

        private Timer rotate;
        public float rotation;

        private bool isVisible;
        Random rand;

        public Ball()
        {
            speed = new Vector2(-350,150);
           
            position =  new Vector2(600, 360);;
            rand = new Random(DateTime.Now.Millisecond);
            isVisible = true;

            rotate = new Timer(16);
            rotate.Elapsed += new ElapsedEventHandler(rotate_Elapsed);
            rotate.Start();
        }

        void rotate_Elapsed(object sender, ElapsedEventArgs e)
        {
            rotation +=  0.10471976f;
            if (rotation >= Math.PI * 2)
                rotation = 0;
        }

        private void UpdateBounds()
        {
            Vector3 spherePosition = new Vector3(position.X + (texture.Width / 2), position.Y + (texture.Height / 2), 0);
            bounds = new BoundingSphere(spherePosition, 20);
        }
        public void LoadContent(ContentManager content, String text)
        {
            texture = content.Load<Texture2D>(text);
            UpdateBounds();
        }

        public float MoveToRight()
        {
            speed.X = Math.Min(Math.Abs(speed.X * 1.15f), MaxSpeed);
            return speed.X;
        }

        public float MoveToLeft()
        {
            speed.X = -Math.Abs(Math.Max(-speed.X * 1.15f,-MaxSpeed));
            return speed.X;
        }
        
        public BoundingSphere getBoundingSphere() { return bounds; }

        public void Stop()
        {
            isVisible = false;
            speed = new Vector2(0);
        }

        public Vector2 IncreaseSpeed(Vector2 inc)
        {
            speed += inc;
            speed.X = Math.Max(Math.Min(speed.X, MaxSpeed),-MaxSpeed);
            speed.Y = Math.Max(Math.Min(speed.Y, MaxSpeed), -MaxSpeed);
           
            return speed;
        }

        public void SetSpeed(Vector2 newspeed)
        {
            speed = newspeed;
            speed.X = Math.Max(Math.Min(speed.X, MaxSpeed), -MaxSpeed);
            speed.Y = Math.Max(Math.Min(speed.Y, MaxSpeed), -MaxSpeed);
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public Vector2 GetSpeed() { return speed; }

        public float GetSpeedX() { return speed.X; }

        public float GetSpeedY() { return speed.Y; }

        public void Reset(bool left)
        {
            position = new Vector2(rand.Next(600, 680), rand.Next(300, 420));
            if(left)
              speed = new Vector2(-rand.Next(200, 300), rand.Next(100, 200));
            else
                speed = new Vector2(rand.Next(200, 300), rand.Next(100, 200));
        }

        public void Update(double elapsed)
        {
            position+= (speed * (float)(elapsed));
            UpdateBounds();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isVisible)
            {
                Rectangle rec = new Rectangle();
                rec.X = (int)position.X + (texture.Width / 2);
                rec.Y = (int)position.Y + (texture.Height / 2);
                rec.Width = texture.Width;
                rec.Height = texture.Height;
                
                spriteBatch.Draw(texture, rec, null, Color.White, rotation, new Vector2(20,20), SpriteEffects.None, 0f);
            }
        }
    }
}
