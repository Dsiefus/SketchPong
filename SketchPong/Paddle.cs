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
            InvisibilityTimer = new Timer(10000);
            InvisibilityTimer.Elapsed += new ElapsedEventHandler(InvisibilityTimer_Elapsed);
            InvisibilityTimer.AutoReset = false;

            LargerBarTimer = new Timer(20000);
            LargerBarTimer.Elapsed += new ElapsedEventHandler(LargerBarTimer_Elapsed);
            LargerBarTimer.AutoReset = false;
        }

        void LargerBarTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            largerbar = 0;
        }

        void InvisibilityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            visible = true;
        }

        public Paddle(float speedY, Vector2 pos)
        {
            speed = speedY;
            points = 0;
            position = pos;
            InvisibilityTimer = new Timer(10000);
            InvisibilityTimer.Elapsed += new ElapsedEventHandler(InvisibilityTimer_Elapsed);
            InvisibilityTimer.AutoReset = false;

            LargerBarTimer = new Timer(20000);
            LargerBarTimer.Elapsed += new ElapsedEventHandler(LargerBarTimer_Elapsed);
            LargerBarTimer.AutoReset = false;
        }

        public void LoadContent(ContentManager content,string textura)
        {
            texture = content.Load<Texture2D>(textura);
            if (textura == "paddle01")
                player = true;
            bounds = new BoundingBox(new Vector3(position, 0), new Vector3(position.X + texture.Width-15, position.Y + texture.Height-15 + texture.Height*0.5f*largerbar, 0));
        }

        public void setVisible(bool vis) 
        {
            visible = vis;
            if (!visible)
            {
                InvisibilityTimer.Stop();
                InvisibilityTimer.Start();
            }
        }

        public void setLargerBar(int large)    
        {
            largerbar = large;
            if (largerbar==1)
            {
                LargerBarTimer.Stop();
                LargerBarTimer.Start();
            }
        }

        public void setScore(int goals)   {     points = goals;     }

        public void setConsecutiveGoals(int ngoals) { consecutive_goals = ngoals; }

        public void IncreaseCGoals(int ngoals)   {    consecutive_goals += ngoals;   }

        public void IncreaseCGoals()  {     consecutive_goals++;    }

        public int getConsecutiveGoals() { return consecutive_goals; }

        public void IncreaseSpeed()    {     speed += 0.5f;   }

        public void IncrementPoints() 
        { 
            points++;
            if (consecutive_goals >= 3)
                points++;
            consecutive_goals++; 
        }

        public void IncrementPoints(int t) 
        {
            points += t;
            if (consecutive_goals >= 3)
                points += t;
            consecutive_goals += t;
        }

        public int GetPoints()  {    return points;  }

        private void SetPosition(Vector2 position)
        {
            this.position = position;
            if(player)
                bounds = new BoundingBox(new Vector3(position, 0), new Vector3(position.X + texture.Width - 15, position.Y + texture.Height - 15 + texture.Height * 0.5f * largerbar, 0));
            else
               bounds = new BoundingBox(new Vector3(position.X + 15, position.Y + 15, 0), new Vector3(position.X + texture.Width, position.Y + texture.Height + texture.Height * 0.5f * largerbar, 0));
        }

        public Vector2 GetPosition() {   return position;  }

        public void MoveUp(double elapsed)   {  SetPosition(position - new Vector2(0, speed*(float)elapsed));    }

        public void MoveDown(double elapsed)   {   SetPosition(position + new Vector2(0, speed*(float)elapsed));    }
        
        public void EnemyIA(Ball ball,double elapsed,float Width, float Height)
        {
        BoundingSphere boundsBall;

        boundsBall= ball.getBoundingSphere();
            //si la bola esta por encima o debajo de la barra, empezar a seguir la bola
            if (!search_ball) 
                if (boundsBall.Center.Y < (bounds.Min.Y) || boundsBall.Center.Y > (bounds.Max.Y))
                    search_ball = true;

            
            if (search_ball) //solo se empieza a mover un poco antes de la mitad del campo
            {
                         //variable temporal para ver datos en pantalla   
                float modifier;

                modifier = (float)(ball.GetPosition().X /Width) + 0.5f;
                /*    
                 * modificador/multiplicador de la velocidad de la barra del ordenador:
                 * primero calculamos el porcentaje de distancia de la bola a la barra, y le sumamos un 30%
                 * En los siguientes if, miramos si la bola esta mas arriba o mas abajo que (mas o menos)
                 * el centro de la barra, y que no se pase de los bordes de juego.
                 * Entonces miro si la distancia en el eje Y de la bola a la barra es superior a 150, aumento
                 * la velocidad entre un 30-40% proporcionalmente a la distancia
                 * Si además esta a mas de 200, +40%
                 * Si además a más de 250, +70%
                 * Y en todos los casos, si la bola esta por arriba y su velocidad también va hacia arriba (o viceversa)
                 * se aumenta un 20% la velocidad.
                 * 
                 * Un máximo teorico, en el caso que la bola estuviera muy lejos verticalmente, muy cerca horizontalmetne
                 * y fuera alejandose, iria a una velocidad de x3, a la práctica el máximo puntual es de unos x2.5
                 * aunque es solo un momento puntual, a medida que se acerca se reduce la velocidad
                 */

                //bola arriba
                if (boundsBall.Center.Y < ((((bounds.Max.Y + bounds.Min.Y) / 2))) && (bounds.Min.Y > 50))
                {
                   // enemy_moving_up = true;
                    if (((bounds.Min.Y + 50) - boundsBall.Center.Y) > 150)                                        
                        modifier += Math.Min(0.3f, (((bounds.Min.Y + 50) - boundsBall.Center.Y) / 150) * 0.3f);
                    
                    if (((bounds.Min.Y + 50) - boundsBall.Center.Y) > 200)                                            
                        modifier += 0.4f;
                    
                    if (((bounds.Min.Y + 50) - boundsBall.Center.Y) > 250)                      
                        modifier += 0.7f;
                    
                    if (ball.GetSpeed().Y < 0)
                        modifier += 0.2f;
                    
                    this.MoveUp(modifier * elapsed);
                }
                else //bola abajo
                    if (boundsBall.Center.Y > ((bounds.Max.Y + bounds.Min.Y) / 2) && (bounds.Max.Y < (Height - 40)))
                    {
                        modifier = (float)(ball.GetPosition().X / Width) + 0.3f;
                       
                        if (boundsBall.Center.Y - (bounds.Max.Y - 50) > 150)                                                
                            modifier += Math.Min(0.3f, ((boundsBall.Center.Y - (bounds.Max.Y - 50)) / 150) * 0.3f);
                        
                        if (boundsBall.Center.Y - (bounds.Max.Y - 50) > 200)                                               
                            modifier += 0.4f;
                        
                        if (boundsBall.Center.Y - (bounds.Max.Y - 50) > 250)                                             
                            modifier += 0.7f;
                        
                        if (ball.GetSpeed().Y > 0)
                            modifier += 0.2f;
                        
                        this.MoveDown(modifier * elapsed);
                    }

                if (Math.Abs(boundsBall.Center.Y - ((bounds.Max.Y + bounds.Min.Y) / 2)) < 20)
                    search_ball = false;
            }           
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
