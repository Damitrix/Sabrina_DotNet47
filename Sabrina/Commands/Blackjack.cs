using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Sabrina.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
    [Group("game"), Aliases("i")]
    internal class Game
    {
        private const string HitStayRegex = "\\b[Hh][Ii][Tt]?\\b|\\b[Ss][Tt][Aa][Nn][Dd]?\\b";
        private const string HitRegex = "[Hh][Ii][Tt]?";
        private const string StandRegex = "[Ss][Tt][Aa][Nn][Dd]?";

        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
        private const string YesRegex = "[Yy][Ee]?[Ss]?";
        private const string NoRegex = "[Nn][Oo]?";

        private Dependencies dep;

        public Game(Dependencies d)
        {
            this.dep = d;
        }


        //Command for the bot
        [Command("blackjack"), Description("Play BlackJack to gamble for an Orgasm")]
        public async Task Blackjack(CommandContext ctx, int betEdges)
        {
            betEdges = 5;
            //Create a new Game and Deal to Player and House
            var game = new BlackJackGame(betEdges);
            bool playerHolds = false;

            await ctx.RespondAsync("Game Start!\n");

            string drawingsText = "";

            drawingsText +=
                $"You draw a {game.PlayerCards[0].Variation.ToString()} {game.PlayerCards[0].Value.ToString()}\n";
            drawingsText += $"The House draws a {game.HouseCards[0].Variation.ToString()} {game.HouseCards[0].Value.ToString()}\n";

            drawingsText +=
                $"You draw a {game.PlayerCards[1].Variation.ToString()} {game.PlayerCards[1].Value.ToString()}\n";
            drawingsText += $"The House draws a Card.\n";

            await ctx.RespondAsync(drawingsText);

            if (BlackJackGame.IsBlackJack(game.PlayerCards) && BlackJackGame.IsBlackJack(game.HouseCards))
            {
                //Both Blackjack, push
                await ctx.RespondAsync("It's a Draw!");
                return;
            }

            if (BlackJackGame.IsBlackJack(game.HouseCards))
            {
                //House Blackjack, House wins
                await ctx.RespondAsync("House wins.");
                return;
            }

            //Loop as long as Player does not hold
            while (!game.PlayerBusts && !playerHolds && !BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                await ctx.RespondAsync("Hit or Stand?");
                var m = await dep.Interactivity.WaitForMessageAsync(
                x => x.Channel.Id == ctx.Channel.Id
                     && x.Author.Id == ctx.Member.Id
                     && Regex.IsMatch(x.Content, HitStayRegex));

                if (Regex.IsMatch(m.Message.Content, HitRegex))
                {
                    game.DrawCardPlayer();
                    await ctx.RespondAsync($"You got a {game.PlayerCards[game.PlayerCards.Count - 1].Variation.ToString()} {game.PlayerCards[game.PlayerCards.Count - 1].Value.ToString()}");
                }
                    
                else if (Regex.IsMatch(m.Message.Content, StandRegex))
                {
                    await ctx.RespondAsync("You Stand.");
                    playerHolds = true;
                }
            }

            if (game.PlayerBusts)
            {
                //House Wins#
                await ctx.RespondAsync("You bust!");
                return;
            }

            if (BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                if (game.HouseCards[0].Value == BlackJackGame.Card.CardValue.Ace)
                {
                    //Ask for even

                    await ctx.RespondAsync("You have a Blackjack, but the House has an Ace, do you want to take even money?");
                    var m = await dep.Interactivity.WaitForMessageAsync(
                        x => x.Channel.Id == ctx.Channel.Id
                             && x.Author.Id == ctx.Member.Id
                             && Regex.IsMatch(x.Content, ConfirmRegex));

                    if (Regex.IsMatch(m.Message.Content, YesRegex))
                    {
                        //Even Money
                        await ctx.RespondAsync("Alrighty! Even it is.");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync("Good Luck.");
                    }
                }
                else
                {
                    //Player wins with BlackJack (3/2)
                    await ctx.RespondAsync("Blackjack! You win!");
                    return;
                }
            }
            else
            {
                if (game.HouseCards[0].Value == BlackJackGame.Card.CardValue.Ace)
                {
                    //Ask for even

                    await ctx.RespondAsync("House has an Ace, do you want to take insurance?");
                    var m = await dep.Interactivity.WaitForMessageAsync(
                        x => x.Channel.Id == ctx.Channel.Id
                             && x.Author.Id == ctx.Member.Id
                             && Regex.IsMatch(x.Content, ConfirmRegex));

                    if (Regex.IsMatch(m.Message.Content, YesRegex))
                    {
                        //Insurance
                        await ctx.RespondAsync("Alrighty! Insurance it is.");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync("Good Luck.");
                    }
                }
            }

            //Flip House' Card
            await ctx.RespondAsync($"House' Card #2 is a {game.HouseCards[game.HouseCards.Count - 1].Variation.ToString()} {game.HouseCards[game.HouseCards.Count - 1].Value.ToString()}\n");

            //Draw for House while Value is under 17
            while (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards) < 17)
            {
                game.DrawCardHouse();
                await ctx.RespondAsync($"House got a {game.HouseCards[game.HouseCards.Count - 1].Variation.ToString()} {game.HouseCards[game.HouseCards.Count - 1].Value.ToString()}\n");

                if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards) == BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
                {
                    //Draw, no one wins
                    await ctx.RespondAsync("It's a Draw!");
                    return;
                }

                if (BlackJackGame.GetMinSum(game.HouseCards) > 21)
                {
                    //House overshoots, you win
                    await ctx.RespondAsync("House busts. You win!");
                    return;
                }
            }

            if (BlackJackGame.IsBlackJack(game.PlayerCards) && BlackJackGame.IsBlackJack(game.HouseCards))
            {
                //Both Blackjack, push
                await ctx.RespondAsync("It's a Draw!");
                return;
            }

            if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards) == BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
            {
                //Draw, no one wins
                await ctx.RespondAsync("It's a Draw!");
                return;
            }
            
            if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards) > BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
            {
                //House wins
                await ctx.RespondAsync("House wins.");
                return;
            }
            else if(BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                //Player wins cause of more Points AND Blackjack, so Money is 3/2
                await ctx.RespondAsync("You win!");
                return;
            }
            else
            {
                //Player wins cause of more Points
                await ctx.RespondAsync("You win!");
                return;
            }
        }
    }

    internal class BlackJackGame
    {
        public int BetEdges { get; private set; }
        public List<Card> Deck = new List<Card>(104);
        public List<Card> HouseCards = new List<Card>();
        public List<Card> PlayerCards = new List<Card>();
        public bool PlayerBusts = false;
        private Random rnd = new Random();

        public BlackJackGame(int betEdges)
        {
            foreach (Card.CardVariation variation in Enum.GetValues(typeof(Card.CardVariation)))
            {
                foreach (Card.CardValue cardValue in Enum.GetValues(typeof(Card.CardValue)))
                {
                    Deck.Add(new Card(){Variation = variation, Value = cardValue});
                }
            }

            BetEdges = betEdges;
            for (int i = 0; i < 2; i++)
            {
                DrawCardHouse();
                DrawCardPlayer();
            }
        }

        public static bool IsBlackJack(List<Card> cards)
        {
            return GetMaxSumWithoutOvershoot(cards) == 21 && cards.Any(e => e.Value == Card.CardValue.Ace);
        }

        public static bool HasAce(List<Card> cards)
        {
            return cards.Any(e => e.Value == Card.CardValue.Ace);
        }

        public void DrawCardPlayer()
        {
            PlayerCards.Add(Deck.Skip(rnd.Next(Deck.Count)).Take(1).First());
            
            if (GetMinSum(PlayerCards) > 21)
            {
                PlayerBusts = true;
            }
        }

        public void DrawCardHouse()
        {
            HouseCards.Add(Deck.Skip(rnd.Next(Deck.Count)).Take(1).First());
        }

        public static int GetMinSum(List<Card> cards)
        {
            int sum = 0;

            foreach (var card in cards)
            {
                sum += (int)card.Value < 11 ? (int)card.Value : 10;
            }

            return sum;
        }

        public static int GetMaxSum(List<Card>cards)
        {
            int sum = 0;

            foreach (var card in cards)
            {
                if (card.Value == Card.CardValue.Ace)
                {
                    sum += 11;
                    continue;
                }
                sum += (int)card.Value < 11 ? (int)card.Value : 10;
            }

            return sum;
        }

        public static int GetMaxSumWithoutOvershoot(List<Card> cards)
        {
            int sum = 0;

            foreach (var card in cards)
            {
                if (card.Value == Card.CardValue.Ace && sum + 11 <= 21)
                {
                    sum += 11;
                    continue;
                }
                else if (card.Value == Card.CardValue.Ace)
                {
                    sum += 1;
                    continue;
                }
                sum += (int)card.Value < 11 ? (int)card.Value : 10;
            }

            return sum;
        }

        public enum GameOutcome
        {
            Undecided,
            Won,
            Lost
        }

        public class Card
        {
            public CardVariation Variation;
            public CardValue Value;

            public enum CardVariation
            {
                Spades,
                Diamonds,
                Clubs,
                Hearts
            }

            public enum CardValue
            {
                Ace = 1,
                Two = 2,
                Three =  3,
                Four = 4,
                Five = 5,
                Six = 6,
                Seven = 7,
                Eight = 8,
                Nine = 9,
                Ten = 10,
                Jack = 11,
                Queen = 12,
                King = 13
            }
        }
    }
}
