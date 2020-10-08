using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Solitaire_Game
{
    public partial class Form1 : Form
    {
        public Image CardFaceSheet = Image.FromFile(MainPath() + "\\Spritesheet\\Card Faces.png");
        public static Bitmap[,] CardFaces = new Bitmap[4, 13];
        public Image CardBackSheet = Image.FromFile(MainPath() + "\\Spritesheet\\Card Backs.png");
        //public static TimeSpan TimeElapsed = new TimeSpan();
        public static DateTime StartTime;
        public static StatusBarPanel Timerdisp = new StatusBarPanel(), MoveCountdisp = new StatusBarPanel();
        public static Card[] HeldCards = new Card[1];
        public static Bitmap[,] CardBacks = new Bitmap[2, 4];
        public static Deck MainDeck;
        public static Drawn3 Free3;
        public static PlayPile[] PlayPiles;
        public static FinalPile[] FinalPiles;
        public static StatusBar BottomStrip;
        public string Mainpath;
        public decimal CardSizePercent = 0.7M; //Default is .7
        public decimal CardOverlapPercent = 85; //Default is 80
        public static Size FullCardSize = new Size(249, 358), CardSize; //Default Full Size 249,358
        public static int CardBackNum = 0, MoveCount =0;
        private static Point ClickLocation;


        public Form1()
        {
            InitializeComponent();
        }

        public class Card
        {
            public int Number; //0-Ace, 1-9-2-10, 10-Jack, 11-Queen, 12-King
            public int Suit;//0- Spade, 1- Heart,2- Diamond, 3-Club
            public bool FaceUp;
            public int OwnerDeckID; //0 Main Deck, 1-4 final decks, 5-11 Play piles, 13 3 from main deck
            public PictureBox Cardpb;

            //Constructor
            public Card(int CardNum,int CardSuit, int DeckID, Point Location)
            {
                Number = CardNum;
                Suit = CardSuit;
                OwnerDeckID = DeckID;

                Cardpb = new PictureBox();
                if (CardBackNum < 4) 
                { 
                    Cardpb.Image = CardBacks[0, CardBackNum]; 
                }
                else 
                { 
                    Cardpb.Image = CardBacks[1, CardBackNum - 4]; 
                }
                Cardpb.BorderStyle = BorderStyle.FixedSingle;
                Cardpb.SizeMode = PictureBoxSizeMode.StretchImage;
                //Cardpb.Click += Cardpb_Click;
                Cardpb.MouseDown += Cardpb_MouseDown;
                Cardpb.MouseMove += Cardpb_MouseMove;
                Cardpb.MouseUp += Cardpb_MouseUp;
                Cardpb.DragDrop += Cardpb_DragDrop;
                Cardpb.Location = Location;
                Cardpb.BackColor = Color.Transparent;
                Cardpb.Name = ((Suit + 1) * CardNum).ToString() + "PB";
                Cardpb.Tag = ""; //Index in Deck
                Cardpb.Visible = false;
                
                FaceUp = false;
            }

            void Cardpb_DragDrop(object sender, DragEventArgs e)
            {
                PictureBox Sender = (PictureBox)sender;

                //throw new NotImplementedException();
            }

            void Cardpb_MouseMove(object sender, MouseEventArgs e)
            {
                PictureBox Sender = (PictureBox)sender;
                if (e.Button == System.Windows.Forms.MouseButtons.Left && Sender.Location != MainDeck.Location && HeldCards[0] != null)
                {
                    for (int c = 0; c < HeldCards.Length; c++)
                    {
                        HeldCards[c].Cardpb.Left = e.X + HeldCards[c].Cardpb.Left - ClickLocation.X;
                        HeldCards[c].Cardpb.Top = e.Y + HeldCards[c].Cardpb.Top - ClickLocation.Y;
                        HeldCards[c].Cardpb.BringToFront();
                    }
                }
                //throw new NotImplementedException();
            }
            
            void Cardpb_MouseUp(object sender, MouseEventArgs e)
            {
                PictureBox Sender = (PictureBox)sender;
                if (Sender.Location != MainDeck.Location)
                {
                    
                    int IndexinDeck;
                    int.TryParse(Sender.Tag.ToString(), out IndexinDeck);
                    bool InvalidPlacement = true;

                    //Find if card in Dropped Location
                    int c;
                    if (HeldCards[0] != null)
                    {
                        for (c = 0; c < 12; c++)
                        {
                            if (c < 7)
                            {
                                //Check if dropped on Play Pile
                                if (PlayPiles[c].Cards.Length > 0)
                                {
                                    if ((e.Location.X + HeldCards[0].Cardpb.Location.X < (PlayPiles[c].Location.X + CardSize.Width)) && (e.Location.X + HeldCards[0].Cardpb.Location.X > PlayPiles[c].Location.X))
                                    {
                                        if ((e.Location.Y + HeldCards[0].Cardpb.Location.Y < PlayPiles[c].Cards[PlayPiles[c].Cards.Length - 1].Cardpb.Location.Y + CardSize.Height) && (e.Location.Y + HeldCards[0].Cardpb.Location.Y > PlayPiles[c].Cards[PlayPiles[c].Cards.Length - 1].Cardpb.Location.Y))
                                        {
                                            //c = playpile index of possible drop
                                            if (ValidPlacement(HeldCards[0], PlayPiles[c].Cards, PlayPiles[c].DeckID))
                                            {
                                                //Valid Move
                                                InvalidPlacement = false;
                                                    if (HeldCards[0].OwnerDeckID > 4 && HeldCards[0].OwnerDeckID < 13)
                                                    {
                                                        //From Play Pile to Play Pile
                                                        for (int k = 0; k < HeldCards.Length; k++)
                                                        {
                                                            PlayPiles[HeldCards[k].OwnerDeckID - 5].Draw();
                                                            PlayPiles[HeldCards[k].OwnerDeckID - 5].Refresh();
                                                            PlayPiles[c].PlaceonTop(HeldCards[k]);
                                                            PlayPiles[c].Refresh();
                                                        }
                                                    }
                                                    if (HeldCards[0].OwnerDeckID == 13)
                                                    {
                                                        //Free 3 to PLay Pile
                                                        PlayPiles[c].PlaceonTop(Free3.Draw());
                                                        Free3.Refresh();
                                                        PlayPiles[c].Refresh();
                                                    }
                                                    HeldCards = new Card[1];
                                                    break;
                                                
                                            }
                                        }
                                    }
                                }else if((e.Location.X + HeldCards[0].Cardpb.Location.X < PlayPiles[c].Location.X+CardSize.Width && e.Location.X + HeldCards[0].Cardpb.Location.X > PlayPiles[c].Location.X) && (e.Location.Y + HeldCards[0].Cardpb.Location.Y < PlayPiles[c].Location.Y + CardSize.Height&& e.Location.Y + HeldCards[0].Cardpb.Location.Y > PlayPiles[c].Location.Y))
                                {
                                    if (ValidPlacement(HeldCards[0], PlayPiles[c].Cards, PlayPiles[c].DeckID))
                                        {
                                        //King to Empty Play Pile
                                        InvalidPlacement = false;
                                        if (HeldCards[0].OwnerDeckID > 4 && HeldCards[0].OwnerDeckID < 13)
                                        {
                                            //From Play Pile to Empty
                                            for (int k = 0; k < HeldCards.Length; k++)
                                            {
                                                PlayPiles[HeldCards[k].OwnerDeckID - 5].Draw();
                                                PlayPiles[HeldCards[k].OwnerDeckID - 5].Refresh();
                                                PlayPiles[c].PlaceonTop(HeldCards[k]);
                                                PlayPiles[c].Refresh();
                                            }
                                        }
                                        if (HeldCards[0].OwnerDeckID == 13)
                                        {
                                            //Free 3 to Empty
                                            PlayPiles[c].PlaceonTop(Free3.Draw());
                                            Free3.Refresh();
                                            PlayPiles[c].Refresh();
                                        }
                                        HeldCards = new Card[1];
                                        break;
                                    }
                                }
                            }
                            else if (c > 7)
                            {
                                //Check if Dropped on Final Build Stack
                                if ((e.Location.X + HeldCards[0].Cardpb.Location.X < (FinalPiles[c - 8].Location.X + CardSize.Width)) && (e.Location.X + HeldCards[0].Cardpb.Location.X > FinalPiles[c - 8].Location.X))
                                {
                                    if ((e.Location.Y + HeldCards[0].Cardpb.Location.Y < FinalPiles[c - 8].Location.Y + CardSize.Height) && (e.Location.Y + HeldCards[0].Cardpb.Location.Y > FinalPiles[c - 8].Location.Y))
                                    {
                                        if (ValidPlacement(HeldCards[0], FinalPiles[c - 8].Cards, FinalPiles[c-8].DeckID))
                                        {
                                            InvalidPlacement = false;
                                            //Dropped on Final Build Stack
                                            if (HeldCards[0].OwnerDeckID > 4 && HeldCards[0].OwnerDeckID < 13)
                                            {
                                                //From Play Pile to Final
                                                for (int k = 0; k < HeldCards.Length; k++)
                                                {
                                                    PlayPiles[HeldCards[k].OwnerDeckID - 5].Draw();
                                                    PlayPiles[HeldCards[k].OwnerDeckID - 5].Refresh();
                                                    FinalPiles[c - 8].PlaceonTop(HeldCards[k]);
                                                    FinalPiles[c - 8].Refresh();
                                                }
                                            }
                                            else if (HeldCards[0].OwnerDeckID == 13)
                                            {
                                                //Free 3 to Final
                                                FinalPiles[c - 8].PlaceonTop(Free3.Draw());
                                                Free3.Refresh();
                                                FinalPiles[c - 8].Refresh();
                                            }
                                            HeldCards = new Card[1];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (InvalidPlacement)
                        {
                            //Return Card(s) to Original Location
                            if (HeldCards[0].OwnerDeckID > 4 && HeldCards[0].OwnerDeckID < 13)
                            {
                                //Originally from playpile
                                PlayPiles[OwnerDeckID - 5].Refresh();
                                HeldCards = new Card[1];
                            }
                            else if (HeldCards[0].OwnerDeckID == 13)
                            {
                                Free3.Refresh();
                                HeldCards = new Card[1];
                            }
                        }
                    }
                }
                else
                {

                }
                
                //throw new NotImplementedException();
            }
            
            void Cardpb_MouseDown(object sender, MouseEventArgs e)
            {
                ClickLocation = e.Location;

                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    PictureBox Sender = (PictureBox)sender;
                    int IndexinDeck;
                    int.TryParse(Sender.Tag.ToString(), out IndexinDeck);
                    
                    //string[] Tag;
                    //Tag = Sender.Tag.ToString().Split(';');

                    //DeckID = Convert.ToInt32(Tag[0]);
                    

                    if (OwnerDeckID == 0)
                    {
                        //MainDeck Clicked
                        if (MainDeck.Cards.Length > 0)
                        {
                            MoveCount++;
                            while (0 < Free3.Cards.Length)
                            {
                                MainDeck.PlaceonBottom(Free3.Draw());
                            }
                            while (Free3.Cards.Length < 3 && MainDeck.Cards.Length > 0)
                            {
                                Free3.PlaceonTop(MainDeck.Draw());
                            }
                            Free3.Refresh();
                        }
                    }
                    if (OwnerDeckID > 4 && OwnerDeckID < 13)
                    {
                        // Play Pile Card Clicked
                        if (IndexinDeck < PlayPiles[OwnerDeckID - 5].Cards.Length - 1)
                        {
                            if (PlayPiles[OwnerDeckID - 5].Cards[IndexinDeck].FaceUp)
                            {
                                //Grab Multiple
                                HeldCards = new Card[(PlayPiles[OwnerDeckID - 5].Cards.Length) - IndexinDeck];
                                for (int c = 0; c+IndexinDeck < PlayPiles[OwnerDeckID - 5].Cards.Length; c++)
                                {
                                    HeldCards[c] = PlayPiles[OwnerDeckID - 5].Cards[IndexinDeck + c];
                                }
                            }
                        }
                        else
                        {
                            //Grab Single 
                            HeldCards[0] = PlayPiles[OwnerDeckID - 5].Cards[IndexinDeck];
                        }
                        //if(MainDeck.Cards.Length>5)

                    }
                    if (OwnerDeckID == 13)
                    {
                        //Free 3 card clicked
                        if(Free3.Cards[Free3.Cards.Length-1].Number == Number && Free3.Cards[Free3.Cards.Length-1].Suit == Suit)
                        {
                            //Top Card of Free 3 clicked
                            HeldCards[0] = Free3.Cards[Free3.Cards.Length - 1];
                        }
                    }
                }
                //throw new NotImplementedException();
            }


            /* Deprecated
            void Cardpb_Click(object sender, EventArgs e)
            {
                //Card Click Event
                if (OwnerDeckID == 0)
                {
                    //MainDeck Clicked
                    MainDeck.Draw(3);
                }
                if (OwnerDeckID > 4 && OwnerDeckID < 13)
                {
                    //PlayPile Clicked

                }
            }
            */
            public void Flip()
            {
                FaceUp = !FaceUp;
                if (Cardpb.Visible == true)
                {
                    if (FaceUp)
                    {
                        Cardpb.Image = CardFaces[Suit, Number];
                    }
                    else
                    {
                        if (CardBackNum < 4) 
                        { 
                            Cardpb.Image = CardBacks[0, CardBackNum];
                        }
                        else 
                        { 
                            Cardpb.Image = CardBacks[1, CardBackNum - 4]; 
                        }
                    }
                }
                Cardpb.Refresh();
            }

        }

        public class Deck
        {
            public Card[] Cards;
            public Point Location;
            private int DeckID;
            //Constructor
            public Deck(Point location, int deckID,int DeckSize=1)
            {
                Location = location;
                DeckID = deckID;
                if(DeckSize == 52)
                {
                    Cards = new Card[52];
                    for (int c = 0; c < 52; c++)
                    { //13,26,39,51
                        if (c <= 12){Cards[c] = new Card(c, 0,0,Location);}
                        if (c >= 13 && c <= 25) { Cards[c] = new Card(c - 13, 1,0,Location); }
                        if (c >= 26 && c <= 38) { Cards[c] = new Card(c - 26, 2,0,Location); }
                        if (c >= 39 && c <= 51) { Cards[c] = new Card(c - 39, 3,0,Location); }
                    }
                }
            }

            public Card Draw()
            {
                
                    if (Cards.Length > 0)
                    {
                        Card TopCard = Cards[Cards.Length - 1];
                        Array.Resize(ref Cards, Cards.Length - 1);
                        if (Cards.Length > 0)
                        {
                            Cards[Cards.Length - 1].Cardpb.Visible = TopCard.Cardpb.Visible;
                            Cards[Cards.Length - 1].Cardpb.Size = TopCard.Cardpb.Size;
                        }
                        Refresh();
                        return TopCard;
                    }
                    else
                    {
                        //MessageBox.Show("Error attempting draw from Empty Deck");
                        throw new System.ArgumentException("Deck out of cards");
                    }
            }
            /* Deprecated
            public void Draw3()
            {
                int c=0;
                bool DeckEmpty = false;
                while (c < 3 && !DeckEmpty)
                {
                    try
                    {
                        Free3.PlaceonTop(Draw());
                        c++;
                    }
                    catch (System.ArgumentException ex)
                    {
                        DeckEmpty = true; 
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            */

            public void PlaceonTop(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                card.OwnerDeckID = DeckID;
                card.Cardpb.Tag = (Cards.Length-1).ToString();
                card.Cardpb.BringToFront();
                Cards[Cards.Length - 1] = card;
                Refresh();
            }

            public void PlaceonBottom(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                for (int c = Cards.Length-1; c > 0; c--)
                {
                    Cards[c] = Cards[c - 1];
                }
                card.OwnerDeckID = DeckID;
                if (card.FaceUp)
                {
                    card.Flip();
                }
                Cards[0] = card;
                Refresh();
            }

            public void Shuffle()
            {
                Random rnd = new Random();
                Card[] TMPCards = new Card[Cards.Length];
                int Index;
                for (int c = 0; c < TMPCards.Length; c++)
                {
                    Index = rnd.Next(Cards.Length - 1);
                    TMPCards[c] = Cards[Index];
                    Cards[Index] = Cards[Cards.Length - 1];
                    Array.Resize(ref Cards, Cards.Length - 1);
                }
                Cards = TMPCards;
            }

            public void Refresh()
            {
                for (int c = 0; c < Cards.Length; c++)
                {
                    Cards[c].Cardpb.Visible = false;
                    Cards[c].Cardpb.Location = Location;
                    Cards[c].Cardpb.Size = CardSize;
                    if(c == Cards.Length-1)
                    {
                        Cards[c].Cardpb.Visible = true;
                    }
                }
            }
        }

        public class PlayPile
        {
            public Card[] Cards;
            public Point Location;
            public int DeckID;
            private int CardYOverlap;
            public PictureBox pbOutline = new PictureBox();

            //Constructor
            public PlayPile(Point location, int ID, int CardOverlapY)
            {
                DeckID = ID;
                Location = location;
                Cards = new Card[0];
                CardYOverlap = CardOverlapY;
                pbOutline.Size = CardSize;
                pbOutline.Location = Location;
                pbOutline.BorderStyle = BorderStyle.FixedSingle;
                pbOutline.Visible = true;
                pbOutline.Name = "PB PlayPile "+ID;
            }

            public void PlaceonTop(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                card.OwnerDeckID = DeckID;
                card.Cardpb.Tag = (Cards.Length - 1).ToString();
                Cards[Cards.Length - 1] = card;
            }

            public Card Draw()
            {

                if (Cards.Length > 0)
                {
                    Card TopCard = Cards[Cards.Length - 1];
                    Array.Resize(ref Cards, Cards.Length - 1);
                    return TopCard;
                }
                else
                {
                    //MessageBox.Show("Error attempting draw from Empty Deck");
                    throw new System.ArgumentException("Deck out of cards");
                }
            }

            public void Refresh()
            {
                for (int c = 0; c < Cards.Length; c++)
                {
                    Cards[c].Cardpb.Location = new Point(Location.X,(Location.Y+(c*CardYOverlap)));
                    Cards[c].Cardpb.BringToFront();
                    if (c == Cards.Length - 1 && !Cards[Cards.Length - 1].FaceUp)
                    {
                        Cards[Cards.Length - 1].Flip();
                    }
                }
            }

            /*
            public void PlaceonBottom(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                for (int c = 1; c < Cards.Length; c++)
                {
                    Cards[c] = Cards[c - 1];
                }
                card.OwnerDeckID = DeckID;
                Cards[0] = card;
            }
            */

        }

        public class Drawn3
        {
            public Card[] Cards;
            public Point Location;
            public int DeckID;
            public int CardXOverlap;

            //Constructor
            public Drawn3(Point location, int CardOverlapX, int DeckId = 13)
            {
                Cards = new Card[0];
                Location = location;
                CardXOverlap = CardOverlapX;
                DeckID = DeckId;
            }

            public void PlaceonTop(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                card.Flip();
                card.Cardpb.Visible = true;
                card.OwnerDeckID = 13;
                card.Cardpb.BackColor = Color.Transparent;
                card.Cardpb.BringToFront();
                Cards[Cards.Length - 1] = card;
            }

            public Card Draw()
            {

                if (Cards.Length > 0)
                {
                    Card TopCard = Cards[Cards.Length - 1];
                    Array.Resize(ref Cards, Cards.Length - 1);
                    if (Cards.Length > 0)
                    {
                        Cards[Cards.Length - 1].Cardpb.Visible = TopCard.Cardpb.Visible;
                        Cards[Cards.Length - 1].Cardpb.Size = TopCard.Cardpb.Size;
                    }
                    return TopCard;
                }
                else
                {
                    //MessageBox.Show("Error attempting draw from Empty Deck");
                    throw new System.ArgumentException("Deck out of cards");
                }
            }

            public void Refresh()
            {
                for (int c = 0; c < Cards.Length; c++)
                {
                    Cards[c].Cardpb.Location = new Point(Location.X + (c * CardXOverlap), Location.Y);
                    Cards[c].Cardpb.BringToFront();
                }
            }
        }

        public class FinalPile
        {
            public Card[] Cards;
            public Point Location;
            public int DeckID;
            public PictureBox pbOutline = new PictureBox();

            //Constructor
            public FinalPile(Point location, int DeckId )
            {
                Location = location;
                pbOutline.Location = Location;
                pbOutline.Visible = true;
                pbOutline.BorderStyle = BorderStyle.FixedSingle;
                pbOutline.Size = CardSize;
                Cards = new Card[0];
                DeckID = DeckId;
            }

            public void PlaceonTop(Card card)
            {
                Array.Resize(ref Cards, Cards.Length + 1);
                if (!card.FaceUp)
                { card.Flip(); }
                card.Cardpb.Visible = true;
                card.OwnerDeckID = DeckID;
                card.Cardpb.BackColor = Color.Transparent;
                card.Cardpb.BringToFront();
                Cards[Cards.Length - 1] = card;
            }

            public Card Draw()
            {

                if (Cards.Length > 0)
                {
                    Card TopCard = Cards[Cards.Length - 1];
                    Array.Resize(ref Cards, Cards.Length - 1);
                    if (Cards.Length > 0)
                    {
                        Cards[Cards.Length - 1].Cardpb.Visible = TopCard.Cardpb.Visible;
                        Cards[Cards.Length - 1].Cardpb.Size = TopCard.Cardpb.Size;
                    }
                    return TopCard;
                }
                else
                {
                    //MessageBox.Show("Error attempting draw from Empty Deck");
                    throw new System.ArgumentException("Deck out of cards");
                }
            }

            public void Refresh()
            {
                for (int c = 0; c < Cards.Length; c++)
                {
                    Cards[c].Cardpb.Location = new Point(Location.X, Location.Y);
                    Cards[c].Cardpb.Visible = false;
                    Cards[c].Cardpb.BringToFront();
                    if (c == Cards.Length - 1)
                    {
                        Cards[c].Cardpb.Visible = true;
                    }
                }
            }
        }

        static string MainPath()
        {
            string path, name;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            name = System.AppDomain.CurrentDomain.FriendlyName.Substring(0, System.AppDomain.CurrentDomain.FriendlyName.IndexOf('.'));
            while (!path.EndsWith("\\" + name))
            {
                path = System.IO.Directory.GetParent(path).ToString();
            }
            return path;
        }

        private void SetupGame()
        {
            //TimeElapsed = TimeSpan.Zero;
            MoveCount = 0;
            MainDeck = new Deck(new Point( 10, menuStrip1.Height +10),0, 52);
            PlayPiles = new PlayPile[7];
            Free3 = new Drawn3(new Point(MainDeck.Location.X+CardSize.Width+10, MainDeck.Location.Y),Convert.ToInt32(CardSize.Width*((100-CardOverlapPercent)/100)));
            Free3.Cards = new Card[0];
            FinalPiles = new FinalPile[4];
            BottomStrip = new StatusBar();
            Timerdisp = new StatusBarPanel();
            MoveCountdisp = new StatusBarPanel();

            this.Controls.Add(BottomStrip);
            BottomStrip.Visible = true;
            BottomStrip.Panels.Add(Timerdisp);
            BottomStrip.Panels.Add(MoveCountdisp);
            Timerdisp.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            Timerdisp.Alignment = HorizontalAlignment.Right;
            Timerdisp.AutoSize = StatusBarPanelAutoSize.Contents;
            MoveCountdisp.Alignment = HorizontalAlignment.Left;
            MoveCountdisp.AutoSize = StatusBarPanelAutoSize.Contents;
            BottomStrip.ShowPanels = true;
            BottomStrip.Invalidated += BottomStrip_Refresh;

            for (int c = 0; c < 52; c++)
            {
                this.Controls.Add(MainDeck.Cards[c].Cardpb);
            }
            MainDeck.Shuffle();
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Visible = true;
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Size = CardSize;

            for (int c = 0; c < 7; c++)
            {
                PlayPiles[c] = new PlayPile(new Point(MainDeck.Location.X + (c*(CardSize.Width+((this.Width/7)-CardSize.Width))), MainDeck.Location.Y + CardSize.Height + 10), c+5, Convert.ToInt32(CardSize.Height*((100-CardOverlapPercent)/100)));
                this.Controls.Add(PlayPiles[c].pbOutline);
                PlayPiles[c].Cards = new Card[0];
            }

            PlayPiles[0].PlaceonTop(MainDeck.Draw());
            PlayPiles[0].Refresh();

            PlayPiles[1].PlaceonTop(MainDeck.Draw());
            PlayPiles[1].PlaceonTop(MainDeck.Draw());
            PlayPiles[1].Refresh();

            for (int c = 0; c < 3; c++)
            {
                PlayPiles[2].PlaceonTop(MainDeck.Draw());
            }
            PlayPiles[2].Refresh();

            for (int c = 0; c < 4; c++)
            {
                PlayPiles[3].PlaceonTop(MainDeck.Draw());
            }
            PlayPiles[3].Refresh();

            for (int c = 0; c < 5; c++)
            {
                PlayPiles[4].PlaceonTop(MainDeck.Draw());
            }
            PlayPiles[4].Refresh();

            for (int c = 0; c < 6; c++)
            {
                PlayPiles[5].PlaceonTop(MainDeck.Draw());
            }
            PlayPiles[5].Refresh();

            for (int c = 0; c < 7; c++)
            {
                PlayPiles[6].PlaceonTop(MainDeck.Draw());
            }
            PlayPiles[6].Refresh();

            for (int c=0; c<4;c++)
            {
                FinalPiles[c] = new FinalPile(new Point(this.Width-(15*(c+1))-(CardSize.Width*(c+1)), MainDeck.Location.Y),c+1);
                this.Controls.Add(FinalPiles[c].pbOutline);
                FinalPiles[c].Cards = new Card[0];
                FinalPiles[c].Refresh();
            }

            StartTime = DateTime.Now;
            timer1.Enabled = true;

            /*
            for (int c = 0; c < 7; c++)
            {
                PlayPiles[c].Cards[PlayPiles[c].Cards.Length - 1].Flip();
            }
            */

        }

        private void LoadSettings()
        {
            string Readline;
            string[] Split;
            StreamReader Reader = new StreamReader(Mainpath + "\\Details.txt");
            Readline = Reader.ReadLine();
            Split = Readline.Split('=');
            int.TryParse(Split[Split.Length - 1], out CardBackNum);
        }

        private void LoadImages()
        {
            //System.IO.File.WriteAllText(MainPath() + "\\Spritesheet\\test.txt", "Test");
            //Cut Card faces from sheet
            //Card Face AR = 0.67
            //Image CardFaceSheet = Image.FromFile(MainPath() + "\\Spritesheet\\Card Faces.png");
            //Bitmap[,] CardFaces = new Bitmap[4, 13];
            int XOffset, YOffset;
            for (int c = 0; c < 4; c++)
            {
                for (int k = 0; k < 13; k++)
                {
                    CardFaces[c, k] = new Bitmap((int)360, (int)539);
                    Graphics g = Graphics.FromImage(CardFaces[c, k]);
                    XOffset = (((k + 1) * 30) + (k * 360));
                    YOffset = (((c + 1) * 30) + (c * 539));
                    g.DrawImage(CardFaceSheet, new Rectangle(0, 0, 360, 539), new Rectangle(XOffset, YOffset, 360, 539), GraphicsUnit.Pixel);
                    g.Dispose();
                }
            }
            //Cut Card Backs
            //Card Back AR = 0.72
            //Image CardBackSheet = Image.FromFile(MainPath() + "\\Spritesheet\\Card Backs.png");
            //Bitmap[,] CardBacks = new Bitmap[2,4];
            for (int c = 0; c < 2; c++)
            {
                for (int k = 0; k < 4; k++)
                {
                    CardBacks[c, k] = new Bitmap((int)256, (int)358);
                    Graphics g = Graphics.FromImage(CardBacks[c, k]);
                    XOffset = (31 + (k * 25) + (k * 256));
                    YOffset = (18 + (c * 21) + (c * 358));
                    g.DrawImage(CardBackSheet, new Rectangle(0, 0, 256, 358), new Rectangle(XOffset, YOffset, 256, 358), GraphicsUnit.Pixel);
                    g.Dispose();
                }
            }
        }

        public static bool ValidPlacement(Card NewCard, Card[] Cards, int NewDeckID)
        {
            if (NewDeckID > 4 && NewDeckID < 13)
            {
                //Playpile placement rules
                if (Cards.Length > 0)
                {
                    if (((NewCard.Suit == 0 || NewCard.Suit == 3) && (Cards[Cards.Length - 1].Suit == 1 || Cards[Cards.Length - 1].Suit == 2)) || ((NewCard.Suit == 1 || NewCard.Suit == 2) && (Cards[Cards.Length - 1].Suit == 0 || Cards[Cards.Length - 1].Suit == 3)))
                    {
                        if (NewCard.Number == Cards[Cards.Length - 1].Number - 1)
                        {
                            MoveCount++;
                            BottomStrip.Refresh();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (NewCard.Number == 12 && Cards.Length == 0)
                {
                    MoveCount++;
                    BottomStrip.Refresh();
                    return true;
                }
            }else if(NewDeckID > 0 && NewDeckID <5)
            {
                //Final Pile Placement Rules
                if (FinalPiles[NewDeckID - 1].Cards.Length == 0 && NewCard.Number == 0)
                {
                    MoveCount++;
                    BottomStrip.Refresh();
                    return true;
                }else if( FinalPiles[NewDeckID-1].Cards.Length >0 && (NewCard.Number == FinalPiles[NewDeckID-1].Cards[FinalPiles[NewDeckID - 1].Cards.Length-1].Number+1) && NewCard.Suit == FinalPiles[NewDeckID-1].Cards[0].Suit)
                {
                    MoveCount++;
                    BottomStrip.Refresh();
                    return true;
                }else
                {
                    return false;
                }

            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Mainpath = MainPath();
            LoadImages();
            CardSize.Height = Convert.ToInt32(Convert.ToDecimal(FullCardSize.Height) * CardSizePercent);
            CardSize.Width = Convert.ToInt32(Convert.ToDecimal(FullCardSize.Width) * CardSizePercent);
            /*3 Aces set for test
            pictureBox1.Image = CardFaces[0, 0];
            pictureBox2.Image = CardFaces[1, 0];
            pictureBox3.Image = CardFaces[2, 0];
             */
            /*3 Backs Set for test
            pictureBox1.Image = CardBacks[0,0];
            pictureBox2.Image = CardBacks[0,3];
            pictureBox3.Image = CardBacks[1,1];
            */
            /*
            MainDeck = new Deck(52);

            MainDeck.Shuffle();
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Visible = true;
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Size = CardSize;
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Location = new Point(10, 10); ;
            this.Controls.Add(MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb);
            MainDeck.Cards[MainDeck.Cards.Length - 1].Flip();
            MainDeck.Cards[MainDeck.Cards.Length - 1].Cardpb.Refresh();
            pictureBox1.Image = CardFaces[MainDeck.Cards[MainDeck.Cards.Length - 1].Suit, MainDeck.Cards[MainDeck.Cards.Length - 1].Number];
            this.Refresh();
             */

            SetupGame();
        }

        private void resetGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (Free3.Cards.Length > 1)
            {
                Free3.Cards[Free3.Cards.Length-1].Cardpb.Dispose();
                Free3.Draw();
            }
            for (int c = 0; c < 11; c++)
            {
                if (c < 4)
                {
                    while (FinalPiles[c].Cards.Length > 0)
                    {
                        FinalPiles[c].Cards[FinalPiles[c].Cards.Length-1].Cardpb.Dispose();
                        FinalPiles[c].Draw();
                    }
                }
                else
                {
                    while(PlayPiles[c-4].Cards.Length>0)
                    {
                        PlayPiles[c - 4].Cards[PlayPiles[c - 4].Cards.Length - 1].Cardpb.Dispose();
                        PlayPiles[c - 4].Draw();
                    }
                }
            }
            this.Controls.Remove(BottomStrip);
                SetupGame();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Timerdisp.Text = (DateTime.Now - StartTime).ToString("hh\\:mm\\:ss");
            BottomStrip.Refresh();
        }

        private void BottomStrip_Refresh(object sender, EventArgs e)
        {
            MoveCountdisp.Text = "Moves: " + MoveCount;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


    }
}
