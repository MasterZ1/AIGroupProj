using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

// Tools
public static class EnumerableUtilities
{
    public static IEnumerable<int> RangeStep(int start, int stop, int step = 1)
    {
        if (step == 0)
            throw new ArgumentException("Parameter step cannot equal zero.");

        if (start < stop && step > 0)
        {
            for (var i = start; i < stop; i += step)
            {
                yield return i;
            }
        }
        else if (start > stop && step < 0)
        {
            for (var i = start; i > stop; i += step)
            {
                yield return i;
            }
        }
    }

    public static IEnumerable<int> RangeStep(int stop)
    {
        return RangeStep(0, stop);
    }

    public static bool Between(this int num, int lower, int upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }
}



public class GameManager : MonoBehaviour
{
    public int depth = 20;

    private string state;
    private List<Tuple<int, int, int>> possible_goals = new List<Tuple<int, int, int>>();
    private List<string> box_won = new List<string>();
    private int bot_move;
    private Tuple<int, int> user_input;
    private Tuple<int, int> user_move;
    private bool user_moved = false;
    private bool gameOver = false;
    

    // Game Functions
    int get_index(int x, int y)
    {
        x -= 1;
        y -= 1;
        return (((int)x / 3) * 27) + ((x % 3) * 3) + (((int)y / 3) * 9) + (y % 3);
    }

    int box(int x, int y)
    {
        return (int)get_index(x, y) / 9;
    }

    int next_box(int i)
    {
        return i % 9;
    }

    List<int> indicies_of_box(int i)
    {
        List<int> idxs_box = new List<int>();
        foreach (var idx in EnumerableUtilities.RangeStep(i * 9, i * 9 + 9))
        {
            idxs_box.Add(i);
        }
        return idxs_box;
    }

    List<int> from2D(List<List<int>> pi_2D)
    {
        List<int> to1D = new List<int>();
        foreach(List<int> set in pi_2D)
        {
            foreach(int i in set)
            {
                to1D.Add(i);
            }
        }
        return to1D;
    }

    List<int> get_possible_moves(Tuple<int, int> last_move)
    {
        int moveidx;
        if (last_move.Item2 != int.MinValue)
        {
            moveidx = get_index(last_move.Item1, last_move.Item2);
        }
        else
        {
            moveidx = last_move.Item1;
        }

        int box_to_play = next_box(moveidx);
        List<int> idxs = indicies_of_box(box_to_play);
        if (box_won[box_to_play] != ".")
        {
            List<List<int>> pi_2d = new List<List<int>>();
            for (int i = 0; i < 9; i++)
            {
                if (box_won[i] == ".")
                {
                    pi_2d.Add(indicies_of_box(i));
                }
            }
            return from2D(pi_2d);
        }
        else
        {
            return idxs;
        }
    }

    bool isValidInput(string state, Tuple<int, int> move)
    {
        if (!(EnumerableUtilities.Between(move.Item1, 0, 10) && EnumerableUtilities.Between(move.Item1, 0, 10)))
            return false;
        if (box_won[box(move.Item1, move.Item2)] != ".")
            return false;
        if (state[get_index(move.Item1, move.Item2)] != '.')
            return false;
        return true;
    }

    Tuple<int, int> check_input(string state, int bot_move)
    {
        List<int> possible_moves = get_possible_moves(new Tuple<int, int>(bot_move, int.MinValue));

        if (bot_move != -1 && !possible_moves.Contains(get_index(user_input.Item1, user_input.Item2)))
        {
            throw new Exception();
        }

        if (!isValidInput(state, user_input))
        {
            throw new Exception();
        }

        return user_input;
    }

    string check_small_box(string box_str)
    {
        foreach(Tuple<int, int, int> idxs in possible_goals)
        {
            if ((box_str[idxs.Item1] == box_str[idxs.Item2] && box_str[idxs.Item1] == box_str[idxs.Item3]) && (box_str[idxs.Item1] != '.'))
            {
                return box_str[idxs.Item1].ToString();
            }
        }
        return ".";
    }

    List<string> update_box_won(string state)
    {
        List<string> temp = new List<string>();
        for (int i = 0; i < 9; i++){temp.Add(".");}
        for (int i = 0; i < 9; i++)
        {
            List<int> idxs_box = indicies_of_box(i);
            string box_str = state.Substring(idxs_box[0], idxs_box[-1] + 1);
            temp[i] = check_small_box(box_str);
        }
        return temp;
    }

    string add_piece(string state, Tuple<int, int> move, char player)
    {
        int moveidx;
        if (move.Item2 != int.MinValue)
        {
            moveidx = get_index(move.Item1, move.Item2);
        }
        else
        {
            moveidx = move.Item1;
        }
        StringBuilder newState = new StringBuilder(state);
        newState[moveidx] = player;
        return newState.ToString();
    }

    List<Tuple<string, int>> recurse(string state, string player, Tuple<int, int> last_move)
    {
        List<string> succ = new List<string>();
        List<int> moves_idx = new List<int>();
        List<int> possible_indexes = get_possible_moves(last_move);
        foreach (int idx in possible_indexes)
        {
            if (state[idx] == '.')
            {
                moves_idx.Add(idx);
                succ.Add(add_piece(state, new Tuple<int, int>(idx, int.MinValue), player[0]));
            }
        }
        List<Tuple<string, int>> pairs = new List<Tuple<string, int>>();
        for (int i = 0; i < succ.Count; i++)
        {
            pairs.Add(new Tuple<string, int>(succ[i], moves_idx[1]));
        }
        return pairs;
    }

    int evaluate_small_box(string box_str, string player)
    {
        int score = 0;
        string three = player + player + player;
        string two = player + player + ".";
        string one = player + "." + ".";
        string three_opp = opponent(player) + opponent(player) + opponent(player);
        string two_opp = opponent(player) + opponent(player) + ".";
        string one_opp = opponent(player) + "." + ".";

        foreach(Tuple<int, int, int> goal in possible_goals)
        {
            string current = box_str[goal.Item1].ToString() + box_str[goal.Item2].ToString() + box_str[goal.Item3].ToString();
            if (current == three)
                score += 100;
            else if (current == two)
                score += 10;
            else if (current == one)
                score += 1;
            else if (current == three_opp)
                score -= 100;
            else if (current == two_opp)
                score -= 10;
            else if (current == one_opp)
                score -= 1;
        }

        return score;
    }

    int evaluate(string state, string player)
    {
        int score = 0;
        string box_wonI = "";
        foreach (string i in box_won) { box_wonI += i; }
        score += evaluate_small_box(box_wonI, player) * 200;
        for (int i = 0; i < 9; i++)
        {
            List<int> idxs = indicies_of_box(i);
            string box_str = state.Substring(idxs[0], idxs[-1] + 1);
            score += evaluate_small_box(box_str, player);
        }
        return score;
    }

    string opponent(string p)
    {
        if (p == "X")
            return "O";
        else
            return "X";
    }

    Tuple<string, int> minimax(string state, Tuple<int, int> last_move, string player, int depth)
    {
        List<Tuple<string, int>> succ = recurse(state, player, last_move);
        Tuple<float, Tuple<string, int>> best_move = new Tuple<float, Tuple<string, int>>(float.NegativeInfinity, null);
        foreach(Tuple<string, int> s in succ)
        {
            float val = min_turn(s.Item1, new Tuple<int, int>(s.Item2, int.MinValue), opponent(player), depth - 1, float.NegativeInfinity, float.PositiveInfinity);
            if (val > best_move.Item1)
            {
                best_move = new Tuple<float, Tuple<string, int>>(val, s);
            }
        }
        return best_move.Item2;
    }

    float min_turn(string state, Tuple<int, int> last_move, string player, int depth, float alpha, float beta)
    {
        string box_wonI = "";
        foreach (string i in box_won) {box_wonI += i;}
        if (depth <= 0 || check_small_box(box_wonI) != ".")
        {
            return evaluate(state, opponent(player));
        }
        List<Tuple<string, int>> succ = recurse(state, player, last_move);
        foreach (Tuple<string, int> s in succ)
        {
            float val = max_turn(s.Item1, new Tuple<int, int>(s.Item2, int.MinValue), opponent(player), depth - 1, alpha, beta);
            if (val > beta)
            {
                beta = val;
            }
            if (alpha >= beta)
                break;
        }
        return beta;
    }

    float max_turn(string state, Tuple<int, int> last_move, string player, int depth, float alpha, float beta)
    {
        string box_wonI = "";
        foreach (string i in box_won) {box_wonI += i;}
        if (depth <= 0 || check_small_box(box_wonI) != ".")
        {
            return evaluate(state, player);
        }
        List<Tuple<string, int>> succ = recurse(state, player, last_move);
        foreach (Tuple<string, int> s in succ)
        {
            float val = min_turn(s.Item1, new Tuple<int, int>(s.Item2, int.MinValue), opponent(player), depth - 1, alpha, beta);
            if (alpha < val)
            {
                alpha = val;
            }
            if (alpha >= beta)
                break;
        }
        return alpha;
    }

    // Unity Functions
    void Start()
    {
        state = string.Concat(Enumerable.Repeat(".", 81));
        possible_goals.Add(new Tuple<int, int, int>(0, 4, 8));
        possible_goals.Add(new Tuple<int, int, int>(2, 4, 6));
        possible_goals.Add(new Tuple<int, int, int>(0, 3, 6));
        possible_goals.Add(new Tuple<int, int, int>(1, 4, 7));
        possible_goals.Add(new Tuple<int, int, int>(2, 5, 8));
        possible_goals.Add(new Tuple<int, int, int>(0, 1, 2));
        possible_goals.Add(new Tuple<int, int, int>(3, 4, 5));
        possible_goals.Add(new Tuple<int, int, int>(6, 7, 8));
        box_won = update_box_won(state);
        bot_move = -1;
        user_input = new Tuple<int, int>(1, 2);
        user_moved = true;
    }

    public void doMove(int x, int y)
    {
        user_input = new Tuple<int, int>(x, y);
        user_moved = true;
    }

    void update_board(string user_state)
    {

    }

    void update_winner(string game_won)
    {
        gameOver = true;
    }

    void Update()
    {
        if (!gameOver)
        {
            // Waiting on player input
            if (user_moved)
            {
                Console.WriteLine("Doing Player");
                try
                {
                    // Check and see if the input was valid
                    user_move = check_input(state, bot_move);
                }
                catch
                {
                    // Invalid input leave and do nothing
                    user_moved = false;
                    return;
                }

                string user_state = add_piece(state, user_move, 'X');
                Console.WriteLine(user_state);
                update_board(user_state);
                box_won = update_box_won(user_state);
                string box_wonI = "";
                foreach (string i in box_won)
                {
                    box_wonI += i;
                }
                string game_won = check_small_box(box_wonI);
                if (game_won != ".")
                {
                    update_winner(game_won);
                    return;
                }

                Console.WriteLine("Doing Bot");
                // Go on ahead mr.bot
                Tuple<string, int> mm = minimax(user_state, user_move, "O", depth);
                string bot_state = mm.Item1;
                Console.WriteLine(bot_state);
                bot_move = mm.Item2;
                state = bot_state;
                box_won = update_box_won(bot_state);
                box_wonI = "";
                foreach (string i in box_won)
                {
                    box_wonI += i;
                }
                game_won = check_small_box(box_wonI);
                if (game_won == ".")
                {
                    update_winner(game_won);
                    return;
                }
                user_moved = false;
            }
        }
    }
}
