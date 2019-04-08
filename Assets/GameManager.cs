using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

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
    private bool user_turn = true;

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
    }

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

    List<int> get_possible_moves(int last_move)
    {
        int box_to_play = next_box(last_move);
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

    public static bool Between(this int num, int lower, int upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }

    bool isValidInput(string state, Tuple<int, int> move)
    {
        if (!(Between(move.Item1, 0, 10) && Between(move.Item1, 0, 10)))
            return false;
        if (box_won[box(move.Item1, move.Item2)] != ".")
            return false;
        if (state[get_index(move.Item1, move.Item2)] != '.')
            return false;
        return true;
    }

    Tuple<int, int> check_input(string state, int bot_move)
    {
        List<int> possible_moves = get_possible_moves(bot_move);

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

    string check_small_box(List<string> box_str)
    {
        foreach(Tuple<int, int, int> idxs in possible_goals)
        {
            if ((box_str[idxs.Item1] == box_str[idxs.Item2] && box_str[idxs.Item1] == box_str[idxs.Item3]) && (box_str[idxs.Item1] != "."))
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
        if (move.Item2 != -999)
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
    }

    public void doMove()
    {
        user_moved = true;
    }

    void update_board(string user_state)
    {

    }

    void update_winner(string game_won)
    {

    }

    void Update()
    {
        // Waiting on player input
        if (user_turn)
        {
            if (user_moved)
            {
                try
                {
                    user_move = check_input(state, bot_move);
                }
                catch
                {
                    // Bad move do nothing
                    user_moved = false;
                    return;
                }

                string user_state = add_piece(state, user_move, 'X');
                update_board(user_state);
                box_won = update_box_won(user_state);
                string game_won = check_small_box(box_won);
                if (game_won != ".")
                {
                    update_winner(game_won);
                }
                user_turn = false;
            }
        }
        
        // Go on ahead mr.bot
        if (user_moved)
        {

        }
    }
}
