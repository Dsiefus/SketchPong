using System;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace sketchPong
{
    public enum GameState
    {
        NONE, LOGO, TITLE, GAME, END,PAUSE
    }

    public enum ItemType { INVERTER,DOUBLER,INVISIBILITY,LARGERBAR}

    public class skPongGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int fixedDrawingWidth = 1280;
        const int fixedDrawingHeight = 720;
        const int MaximumGoals = 10;
        
        int RealGameTime=0; //tiempo total real en segundos jugando


        bool HaveScored = false, player_wins = false; 
        bool inv_controls = false, player_last_hit = false;
        bool doubler_active = false;


        GameState gameState = new GameState();

        Timer GameTimer,GoalTimer; //Timers para el tiempo de juego, tiempo que mostrar mensaje de gol
        Timer InvTimer,NewItemTimer,DoublerTimer; 
        Timer LargerBarTimer;

        ItemType itemType;
       
        SpriteFont gamefont;
        Texture2D paddlePlayer, paddleEnemy, background, frontend, title, logo,Item;
        Vector2 positionPlayer, positionEnemy, positionBall, positionTitle, positionLogo;
        Vector2 positionScore, positionStart,positionTime, positionTemporal,positionMessage,positionPause;
        Vector2 positionWinnerMessage, positionFinalMessage, positionFinalMessage2,positionItem;
       
        BoundingBox boundsPlayer, boundsEnemy;
        BoundingSphere boundsBall,boundsItem;


        //****************************************
        Paddle Enemy, Player;
        Ball ball;
        Input input;
        //****************************************
                
        Random rand = new Random(DateTime.Now.Millisecond);

        public skPongGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = fixedDrawingWidth;
            graphics.PreferredBackBufferHeight = fixedDrawingHeight;
            graphics.ApplyChanges();
            Window.Title = "Sketch Pong";            
            gameState = GameState.NONE;

            InitPositions();

            InitTimers();            

            //****************************************
            Enemy = new Paddle(400,new Vector2(1170,330));
            Player = new Paddle(520,new Vector2(40,340));
            Player.setVisible(true);
            Enemy.setVisible(true);
            ball = new Ball();
            input = new Input();
            //****************************************            
                        
            base.Initialize();
        }

        private void InitPositions()
        {
            //posicion en pantalla de los diferentes elementos
            positionPlayer = new Vector2(50, 340);
            positionEnemy = new Vector2(1170, 330);
            positionBall = new Vector2(600, 360);
            positionTitle = new Vector2(300, 160);
            positionLogo = new Vector2(420, 200);
            positionScore = new Vector2(540, 0);
            positionStart = new Vector2(480, 360);
            positionTime = new Vector2(470, 70);
            positionMessage = new Vector2(540, 220);
            positionPause = new Vector2(460, 300);
            positionWinnerMessage = new Vector2(200, 220);
            positionFinalMessage = new Vector2(125, 300);
            positionFinalMessage2 = new Vector2(250, 370);
            positionTemporal = new Vector2(540, 150);
            positionItem = new Vector2(rand.Next(100, 1100), rand.Next(150, 600));
        }

        private void InitTimers()
        {
            //Diferentes timers con sus handlers para cuando termina cada uno
            GameTimer = InitializeTimer(1000, GameTimer_Elapsed); //inicializamos timers a 1 segundo
            GameTimer.AutoReset = true;

            GoalTimer = InitializeTimer(1000, GoalTimer_Elapsed);

            InvTimer = InitializeTimer(5000,InvTimer_Elapsed); //Item de invertir controles, durante 5 segundos

            DoublerTimer = InitializeTimer(15000, DoublerTimer_Elapsed);

            LargerBarTimer = InitializeTimer(25000, LargerBarTimer_Elapsed);

            NewItemTimer = InitializeTimer(1000, NewItemTimer_Elapsed);            
        }

        private Timer InitializeTimer(int time, Action<object,ElapsedEventArgs> handler)
        {
            Timer timer=new Timer(time);
            timer.Elapsed += new ElapsedEventHandler(handler);
            //ponemos el AutoReset a false, de manera que solo se ejecutara 1 vez la funcion
            timer.AutoReset = false; 
            //cada vez que iniciermos el Timer
            return timer;
        }

        private bool PlayerMovingUp(KeyboardState keyState)
        {
            return (((keyState.IsKeyDown(Keys.W) && !inv_controls) || (keyState.IsKeyDown(Keys.S) && inv_controls)) &&
                   (boundsPlayer.Min.Y > 20));
        }

        private bool PlayerMovingDown(KeyboardState keyState)
        {
            return (((keyState.IsKeyDown(Keys.S) && !inv_controls) || (keyState.IsKeyDown(Keys.W) && inv_controls)) && 
                    (boundsPlayer.Max.Y < (Window.ClientBounds.Height - 30)));
        }

        //funcion principal
        protected override void Update(GameTime gameTime)
        {           
            TimeSpan elapsedTime = gameTime.ElapsedGameTime;
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.F10)) graphics.ToggleFullScreen();
            if (keyState.IsKeyDown(Keys.Escape)) this.Exit();
            
            switch (gameState)
            {
                case GameState.NONE: 
                    {    gameState = GameState.LOGO;  
                    } break;
                case GameState.LOGO:
                    { //mostrar logo 1 segundo
                        if (gameTime.TotalGameTime.Seconds > 1) gameState = GameState.TITLE;
                    } break;
                case GameState.TITLE: 
                    {
                        if (keyState.IsKeyDown(Keys.Enter))
                        {
                            gameState = GameState.GAME;
                            RealGameTime = 0;                                                         
                            GameTimer.Enabled = true;
                            PutNewItem();
                        }
                    } break;
                case GameState.GAME: 
                    {
                        UpdateGame(ref elapsedTime, ref keyState);
                    } break;

                case GameState.PAUSE:
                    {                        
                        EnableAllTimers(false);
                        input.Update();
                        if (input.PressSpace)
                        {                            
                           gameState = GameState.GAME;                           
                           EnableAllTimers(true);
                        }
                    }break;
                case GameState.END:
                    {                        
                        EnableAllTimers(false);
                        input.Update();                      
                        if (input.PressStart)                                                                              
                            RestartGame();              
                        
                    } break;
                default: break;
            }
            base.Update(gameTime);
        }

        private void UpdateGame(ref TimeSpan elapsedTime, ref KeyboardState keyState)
        {
            MovePlayer(ref elapsedTime, ref keyState);
            MoveEnemy(elapsedTime);

            UpdateBounds();
            CheckIntersections(keyState);

            if (boundsBall.Center.X < 50)
                Score(Enemy);
           
            if (boundsBall.Center.X > (Window.ClientBounds.Width - 50))
                Score(Player);          

            ball.Update(elapsedTime.TotalSeconds);
            input.Update();
            
            if (input.PressSpace)
                gameState = GameState.PAUSE;
        }

        private void MoveEnemy(TimeSpan elapsedTime)
        {
            if (ball.GetPosition().X > 600 && ball.GetSpeed().X > 0)
                Enemy.EnemyIA(ball, elapsedTime.TotalSeconds, Window.ClientBounds.Width, Window.ClientBounds.Height);
            if (ball.GetSpeedX() < 0)
                Enemy.GoToCenter(elapsedTime.TotalSeconds, frontend);
           
        }

        private void MovePlayer(ref TimeSpan elapsedTime, ref KeyboardState keyState)
        {
            if (PlayerMovingUp(keyState))
                Player.MoveUp(elapsedTime.TotalSeconds);
            if (PlayerMovingDown(keyState))
                Player.MoveDown(elapsedTime.TotalSeconds);
        }

        private void CheckIntersections(KeyboardState keyState)
        {
            if (boundsPlayer.Intersects(boundsBall))
            {
                ball.MoveToRight();
                //aumentar la velocidad en el eje Y en la direccion del movimiento de la barra
                if (PlayerMovingUp(keyState))
                    ball.IncreaseSpeed(new Vector2(0, -Math.Max(Math.Abs(ball.GetSpeedY()) * 0.2f, 0.3f)));
                if (PlayerMovingDown(keyState))
                    ball.IncreaseSpeed(new Vector2(0, Math.Max(Math.Abs(ball.GetSpeedY()) * 0.2f, 0.3f)));

                player_last_hit = true; //player ultimo en golpear
            }

            if (boundsEnemy.Intersects(boundsBall))
            {
                ball.MoveToLeft();
                player_last_hit = false;
            }

            if (boundsBall.Center.Y < 50 || boundsBall.Center.Y > (Window.ClientBounds.Height - 50))
                ball.SetSpeed(new Vector2(ball.GetSpeedX(), -(ball.GetSpeedY()) * 1.07f));

            if (boundsBall.Intersects(boundsItem))
                ManageItem(itemType);
            
        }

        private void UpdateBounds()
        {
            boundsPlayer = Player.GetBoundingBox();
            boundsEnemy = Enemy.GetBoundingBox();
            boundsBall = ball.getBoundingSphere();
        }

        private void RestartGame()
        {
            gameState = GameState.GAME; 
            EnableAllTimers(true);
            Player.setScore(0);
            Enemy.setScore(0);
            Enemy.setConsecutiveGoals(0);
            Player.setConsecutiveGoals(0);
            RealGameTime = 0;
            HaveScored = false;
        }

        private void EnableAllTimers(bool enable)
        {
            if (enable)
            {
                GameTimer.Start();
                GoalTimer.Start();
                InvTimer.Start();
                NewItemTimer.Start();
                DoublerTimer.Start();
                LargerBarTimer.Start();
            }
            else
            {
                GameTimer.Stop();
                GoalTimer.Stop();
                InvTimer.Stop();
                NewItemTimer.Stop();
                DoublerTimer.Stop();
                LargerBarTimer.Stop();
            }
        }

        private void ManageItem(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.DOUBLER:
                    ball.LoadContent(Content, "ball02");
                    doubler_active = true;
                    DoublerTimer.Start();
                    break;
                case ItemType.INVERTER:
                    //invertimos controles si el jugador ha sido el ultimo en tocar la bola
                    if (player_last_hit)
                    {
                        inv_controls = true;
                        //por si acaso habiamos iniciado antes un InvTimer, reiniciamos el tiempo de invertir
                        InvTimer.Stop();
                        InvTimer.Start();
                    }
                    break;
                case ItemType.INVISIBILITY:
                    if (!player_last_hit)
                        Enemy.setVisible(false);
                    else
                        Player.setVisible(false);
                    break;
                case ItemType.LARGERBAR:
                    if (player_last_hit)
                        Player.setLargerBar(1);
                    else
                        Enemy.setLargerBar(1);
                    break;
            }

            //dibujamos el Item fuera de pantalla
            positionItem.X = -100;
            positionItem.Y = -100;
            boundsItem = new BoundingSphere(new Vector3(positionItem.X + (Item.Width / 2), positionItem.Y + (Item.Height / 2), 0), 5);

            //iniciamos el temporizador para nuevo Item
            NewItemTimer.Start();

        }

        private void Score(Paddle player)
        {
            HaveScored = true;
            GoalTimer.Start();

            //la bola saldra hacia el Player  
            ball.Reset(true);
            if (doubler_active)
                player.IncrementPoints(2);
            else
                player.IncrementPoints();
            if(player.Equals(Player))
                Enemy.setConsecutiveGoals(0);
            else
                Player.setConsecutiveGoals(0);

            //limite de puntos del juego
            if (player.GetPoints() >= MaximumGoals)
            {
                if (player.Equals(Player))
                    player_wins = true;
               
                gameState = GameState.END;
            }
        }

        //funcion para colocar un nuevo item en pantalla
        void PutNewItem()
        {
            itemType = (ItemType)rand.Next(4);
            switch (itemType)
            {
                case ItemType.DOUBLER:
                    Item = Content.Load<Texture2D>("dpuntuation");
                    break;
                case ItemType.INVERTER:
                    Item = Content.Load<Texture2D>("invcontrol");
                    break;
                case ItemType.INVISIBILITY:
                    Item = Content.Load<Texture2D>("invisibility");
                    break;
                case ItemType.LARGERBAR:
                    Item = Content.Load<Texture2D>("growbar");
                    break;
            }
           

            positionItem = new Vector2(rand.Next(100, 1100), rand.Next(150, 600));
            boundsItem = new BoundingSphere(new Vector3(positionItem.X + (Item.Width / 2), positionItem.Y + (Item.Height / 2), 0), 6);
        }

        //funcion de dibujar NO NECESARIO mirar
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

                switch (gameState)
                {
                    case GameState.NONE: { } break;
                    case GameState.LOGO:
                         {
                             spriteBatch.Draw(background, new Vector2(0, 0), Color.Beige);
                             if (gameTime.TotalGameTime.Seconds > 0) spriteBatch.Draw(logo, positionLogo, Color.White);
                         } break;
                    case GameState.TITLE:
                         {
                             spriteBatch.Draw(background, new Vector2(0, 0), Color.Beige);
                             if (gameTime.TotalGameTime.Seconds > 1) spriteBatch.Draw(title, positionTitle, Color.White);
                             if ((gameTime.TotalGameTime.Seconds > 2) && ((gameTime.TotalGameTime.Seconds % 2) == 0)) 
                                spriteBatch.DrawString(gamefont, "press start", positionStart, Color.Red, 0.0f, new Vector2(0, 0), 0.6f, SpriteEffects.None, 0.0f);
                         } break;
                    case GameState.GAME: 
                         {
                             DrawMainElements();
                         } break;
                    case GameState.PAUSE:
                        {
                            DrawMainElements();

                            spriteBatch.DrawString(gamefont, "PAUSE!", positionPause, Color.Red);
                        }break;                        
                    case GameState.END: {
                        DrawMainElements();
                        if(player_wins)
                           spriteBatch.DrawString(gamefont, "Player Wins!", positionWinnerMessage, Color.Red);
                        else
                            spriteBatch.DrawString(gamefont, "Computer Wins!", positionWinnerMessage, Color.Red);

                        spriteBatch.DrawString(gamefont, "Press Enter to restart", positionFinalMessage, Color.Red);
                        spriteBatch.DrawString(gamefont, " or ESC to exit", positionFinalMessage2, Color.Red);
                    } break;
                    default: break;
                }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawMainElements()
        {
            spriteBatch.Draw(background, new Vector2(0, 0), Color.Beige);
            spriteBatch.Draw(frontend, new Vector2(0, 0), Color.White);

            if (HaveScored)
                spriteBatch.DrawString(gamefont, "Goal!", positionMessage, Color.Orange);

            spriteBatch.DrawString(gamefont, Player.GetPoints().ToString() + " - " + Enemy.GetPoints().ToString(), positionScore, Color.Black);

            if (RealGameTime % 60 > 9)
                spriteBatch.DrawString(gamefont, "  0" + (RealGameTime / 60).ToString() + ":" + (RealGameTime % 60).ToString(), positionTime, Color.Blue);
            else
                spriteBatch.DrawString(gamefont, "  0" + (RealGameTime / 60).ToString() + ":0" + (RealGameTime % 60).ToString(), positionTime, Color.Blue);
            
            Player.Draw(spriteBatch);
            Enemy.Draw(spriteBatch);
        
            spriteBatch.Draw(Item, positionItem, Color.White);            
            ball.Draw(spriteBatch);
        }
         

        //funciones para cuando terminan los diferentes timers
        void LargerBarTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Enemy.setLargerBar(0);
            Player.setLargerBar(0);
        }

        void DoublerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            ball.LoadContent(Content, "ball01");
            doubler_active = false;
        }

        void NewItemTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PutNewItem();
        }

        void InvTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            inv_controls = false;
        }

        void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RealGameTime++;
        }

        void GoalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HaveScored = false;
        }

        //NO NECESARIO MIRAR, carga texturas
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            title = Content.Load<Texture2D>("title");
            logo = Content.Load<Texture2D>("logo");
            background = Content.Load<Texture2D>("background");
            frontend = Content.Load<Texture2D>("frontend");
            paddlePlayer = Content.Load<Texture2D>("paddle01");
            paddleEnemy = Content.Load<Texture2D>("paddle02");

            Item = Content.Load<Texture2D>("invcontrol");

            gamefont = Content.Load<SpriteFont>("basic");

            //****************************************
            Enemy.LoadContent(Content, "paddle02");
            Player.LoadContent(Content, "paddle01");
            ball.LoadContent(Content, "ball01");
            //****************************************

            boundsItem = new BoundingSphere(new Vector3(positionItem.X + (Item.Width / 2), 
                                            positionItem.Y + (Item.Height / 2), 0), 5);

        }
        

        protected override void UnloadContent() { }

        // Main Entry Point
        static class skPongProgram
        {
            static void Main(string[] args)
            {
                using (skPongGame game = new skPongGame())
                {
                    game.Run();
                }
            }
        }
    }
}
