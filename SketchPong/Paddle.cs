using System;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace sketchPong
{
    public class Paddle
    {
        private Vector2 position;
        private BoundingBox bounds;
        private int points;
        private float speed;
        private bool visible = false, player = false, search_ball = false;
        private int largerbar = 0, consecutive_goals = 0;
        private Texture2D texture;
        private Timer InvisibilityTimer,LargerBarTimer;
        //private Color color;

        public Paddle()
        {
            speed = 400f;
            points = 0;            
            position = new Vector2(0, 0);
            SetTimers();
        }

        void LargerBarTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            largerbar = 0;
        }

        void InvisibilityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            visible = true;
        }

        private void SetTimers()
        {
            InvisibilityTimer = new Timer(10000);
            InvisibilityTimer.Elapsed += new ElapsedEventHandler(InvisibilityTimer_Elapsed);
            InvisibilityTimer.AutoReset = false;

            LargerBarTimer = new Timer(20000);
            LargerBarTimer.Elapsed += new ElapsedEventHandler(LargerBarTimer_Elapsed);
            LargerBarTimer.AutoReset = false;

        }

        private void RestartTimer(Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        public Paddle(float speedY, Vector2 pos)
        {
            speed = speedY;
            points = 0;
            position = pos;
            SetTimers();
        }

        public void LoadContent(ContentManager content,string textura)
        {
            texture = content.Load<Texture2D>(textura);
            if (textura == "paddle01")
                player = true;
            Vector3 lowerLeft=new Vector3(position, 0);
            Vector3 upperRight=new Vector3(position.X + texture.Width-15, 
                                           position.Y + texture.Height-15 + texture.Height*0.5f*largerbar, 
                                           0);
            bounds = new BoundingBox(lowerLeft,upperRight);
            
        }
        
        public void setVisible(bool vis) 
        {
            visible = vis;
            if (!visible)
            {
                RestartTimer(InvisibilityTimer);
            }
        }

        public void setLargerBar(int large)    
        {
            largerbar = large;
            if (largerbar==1)
            {
                RestartTimer(LargerBarTimer);
            }
        }

        public void setScore(int goals)   {     points = goals;     }

        public void setConsecutiveGoals(int ngoals) { consecutive_goals = ngoals; }

        public int IncreaseCGoals(int ngoals) { consecutive_goals += ngoals; return consecutive_goals; }

        public int IncreaseCGoals() { consecutive_goals++; return consecutive_goals; }

        public int getConsecutiveGoals() { return consecutive_goals; }

        public void SetSpeed(float newspeed) { speed = newspeed; }

        public int IncrementPoints() 
        { 
            points++;
            if (consecutive_goals >= 3)
                points++;
            consecutive_goals++;
            return points;
        }

        public int IncrementPoints(int t) 
        {
            points += t;
            if (consecutive_goals >= 3)
                points += t;
            consecutive_goals += t;

            return points;
        }

        public int GetPoints()  {    return points;  }

        private void SetPosition(Vector2 position)
        {
            this.position = position;
            Vector3 lowerLeft, upperRight;
            if (player)
            {
                lowerLeft = new Vector3(position, 0);
                upperRight = new Vector3(position.X + texture.Width - 15, 
                                         position.Y+texture.Height-15+texture.Height*0.5f*largerbar,
                                         0);
            }
            else
            {
                lowerLeft = new Vector3(position.X + 15, position.Y + 15, 0);
                upperRight = new Vector3(position.X + texture.Width, 
                                        position.Y + texture.Height + texture.Height * 0.5f * largerbar, 
                                        0);
            }
                        
                bounds = new BoundingBox(lowerLeft,upperRight);
        }

        public Vector2 GetPosition() {   return position;  }

        public float GetSpeed() { return speed; }

        public Vector2 MoveUp(double elapsed) { SetPosition(position - new Vector2(0, speed * (float)elapsed)); return position; }

        public Vector2 MoveDown(double elapsed) { SetPosition(position + new Vector2(0, speed * (float)elapsed)); return position; }
        
        public void EnemyIA(Ball ball,double elapsedTime,float Width, float Height)
        {
            BoundingSphere boundsBall;

            boundsBall= ball.getBoundingSphere();
               
            if (!search_ball && (boundsBall.Center.Y < (bounds.Min.Y) || boundsBall.Center.Y > (bounds.Max.Y)))                 
                        search_ball = true;
            
            if (search_ball) //solo se empieza a mover un poco antes de la mitad del campo
            {
                           
                float modifier = (float)(ball.GetPosition().X /Width) + 0.5f; 
                
                if (BallIsUp(ref boundsBall))
                {                   
                    modifier = CalculateModifier(ball, Width, ((bounds.Min.Y + 50) - boundsBall.Center.Y));                    
                    if (ball.GetSpeed().Y < 0)
                        modifier += 0.2f;                    
                    this.MoveUp(modifier * elapsedTime);
                }
               
                if (BallIsDown(Height, ref boundsBall))
                {
                    modifier = CalculateModifier(ball, Width, boundsBall.Center.Y - (bounds.Max.Y - 50));                        
                    if (ball.GetSpeed().Y > 0)
                        modifier += 0.2f;                        
                    this.MoveDown(modifier * elapsedTime);
                }

                if (Math.Abs(boundsBall.Center.Y - ((bounds.Max.Y + bounds.Min.Y) / 2)) < 20)
                    search_ball = false;
            }           
        }

        private bool BallIsDown(float Height, ref BoundingSphere boundsBall)
        {
            return boundsBall.Center.Y > ((bounds.Max.Y + bounds.Min.Y) / 2) && (bounds.Max.Y < (Height - 40));
        }

        private bool BallIsUp(ref BoundingSphere boundsBall)
        {
            return boundsBall.Center.Y < ((((bounds.Max.Y + bounds.Min.Y) / 2))) && (bounds.Min.Y > 50);
        }

        /*    
        * modificador/multiplicador de la velocidad de la barra del ordenador:
        * dist% + 30%               
        * if distY> 150, speed+(30~40%)
        * if > 200, +40%
        * if > 250, +70%
        * if ball is up and move up, +20%
        * MaxSpeed: teoric = x3, real minX.5
        */
        private float CalculateModifier(Ball ball,float Width, float absoluteDist)
        {
            float modifier = (float)(ball.GetPosition().X / Width) + 0.3f;
            if (absoluteDist > 150)
                modifier += Math.Min(0.3f, (absoluteDist / 150) * 0.3f);

            if (absoluteDist > 200)            
                modifier += 0.4f;

            if (absoluteDist > 250)
                modifier += 0.7f; 

            return modifier;

        }
        public BoundingBox GetBoundingBox()  {     return bounds;   }

        public void GoToCenter(double elapsed,Texture2D frontend)
        {
            float center = ((bounds.Max.Y + bounds.Min.Y) / 2);
            float screen_center = frontend.Height / 2;

            //si la barra esta mas abajo respecto el centro del terreno de juego
            if ((center - 10) > screen_center)            
                MoveUp(elapsed); 
            else if ((center + 10) < screen_center)               
                      MoveDown(elapsed);                
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(visible)
                if(largerbar==1)
                    spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(0, 0), new Vector2(1f, 1.5f), SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
