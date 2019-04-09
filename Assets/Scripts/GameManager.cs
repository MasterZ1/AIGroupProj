using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

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
    public int depth = 5;
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject coverPrefab;
    public Material transparentMat;

    private string state;
    private List<Tuple<int, int, int>> possible_goals = new List<Tuple<int, int, int>>();
    private List<string> box_won = new List<string>();
    private int bot_move;
    private Tuple<int, int> user_input;
    private Tuple<int, int> user_move;
    private bool gameOver = false;
    private bool inputSuccess = false;
    private Transform bigHighlight;
    private List<int> boardsWon = new List<int>();
    private List<List<int>> boardspots = new List<List<int>>();
    string user_state;

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
        foreach (var idx in EnumerableUtilities.RangeStep(i * 9, (i * 9) + 9))
        {
            idxs_box.Add(idx);
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
        if (!(EnumerableUtilities.Between(move.Item1, 0, 10) && EnumerableUtilities.Between(move.Item2, 0, 10)))
            return false;
        if (box_won[box(move.Item1, move.Item2)] != ".")
            return false;
        if (state[get_index(move.Item1, move.Item2)] != '.')
            return false;
        return true;
    }

    Tuple<int, int> check_input(string state, int bot_move)
    {
        if (bot_move != -1)
        {
            List<int> possible_moves = get_possible_moves(new Tuple<int, int>(bot_move, int.MinValue));
            if (!possible_moves.Contains(get_index(user_input.Item1, user_input.Item2)))
            {
                throw new Exception();
            }
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

    void updateBoardWin(string winner, int num)
    {
        if (!boardsWon.Contains(num))
            boardsWon.Add(num);
        else
            return;
        
        Transform parent;
        float yAdj = 0;
        if (num == 0)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Left").transform;
            yAdj = 0.9f;
        }
        else if (num == 1)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Middle").transform;
            yAdj = 0.9f;
        }
        else if (num == 2)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Right").transform;
            yAdj = 0.9f;
        }
        else if (num == 3)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Left").transform;
            yAdj = 0.5f;
        }
        else if (num == 4)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Middle").transform;
            yAdj = 0.5f;
        }
        else if (num == 5)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Right").transform;
            yAdj = 0.5f;
        }
        else if (num == 6)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Left").transform;
            yAdj = 0.1f;
        }
        else if (num == 7)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Middle").transform;
            yAdj = 0.1f;
        }
        else
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Right").transform;
            yAdj = 0.1f;
        }

        var spawn = Instantiate(coverPrefab);
        spawn.transform.position = parent.transform.position;
        spawn.transform.position = new Vector3(spawn.transform.position.x, spawn.transform.position.y + yAdj, spawn.transform.position.z - 1);
        if (winner == "X")
            spawn.GetComponent<Renderer>().material = playerPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
        else
            spawn.GetComponent<Renderer>().material = botPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
    }

    List<string> update_box_won(string state)
    {
        List<string> temp = new List<string>();
        for (int i = 0; i < 9; i++){temp.Add(".");}
        for (int k = 0; k < 9; k++)
        {
            List<int> idxs_box = indicies_of_box(k);
            string pr = "";
            foreach(int p in idxs_box) { pr += p + " "; }
            int length = idxs_box[idxs_box.Count - 1] - idxs_box[0] + 1;
            string box_str = state.Substring(idxs_box[0], length);
            string potentialWinner = check_small_box(box_str);
            if (potentialWinner != ".")
                updateBoardWin(potentialWinner, k);
            temp[k] = potentialWinner;
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
            pairs.Add(new Tuple<string, int>(succ[i], moves_idx[i]));
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
        for (int k = 0; k < 9; k++)
        {
            List<int> idxs = indicies_of_box(k);
            int length = idxs[idxs.Count - 1] - idxs[0] + 1;
            string box_str = state.Substring(idxs[0], length);
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
            if (val < beta)
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
        for (int i = 0; i < 9; i++)
        {
            boardspots.Add(new List<int>() { 0 + (i * 8) + i, 1 + (i * 8) + i, 2 + (i * 8) + i,
                                             3 + (i * 8) + i, 4 + (i * 8) + i, 5 + (i * 8) + i,
                                             6 + (i * 8) + i, 7 + (i * 8) + i, 8 + (i * 8) + i });
        }
        box_won = update_box_won(state);
        bot_move = -1;
        bigHighlight = GameObject.Find("Highlight").transform;
        GameObject.Find("Canvas/Action Text").GetComponent<Text>().text = "Red player is deciding where to play...";
    }

    void update_winner(string winner)
    {
        if (winner == "O")
            GameObject.Find("Canvas/Action Text").GetComponent<Text>().text = "Blue player has won the game!!!";
        else
            GameObject.Find("Canvas/Action Text").GetComponent<Text>().text = "Red player has won the game!!!";

        gameOver = true;
        Destroy(GameObject.Find("Highlight"));
    }

    void placeBot(string n, string o)
    {
        for (int r = 1; r < 10; r++)
        {
            for (int c = 1; c < 10; c++)
            {
                if ((n[get_index(r, c)] != o[get_index(r, c)]) && n[get_index(r, c)] == 'O')
                {
                    string parentGridXY = "Grid ";
                    string childGridXY = "Grid ";

                    if (r > 6)
                        parentGridXY += "Bottom ";
                    else if (r > 3)
                        parentGridXY += "Middle ";
                    else
                        parentGridXY += "Top ";

                    if (c > 6)
                        parentGridXY += "Right";
                    else if (c > 3)
                        parentGridXY += "Middle";
                    else
                        parentGridXY += "Left";

                    if (r  % 3 == 0)
                        childGridXY += "Bottom ";
                    else if (r % 3 == 2)
                        childGridXY += "Middle ";
                    else
                        childGridXY += "Top ";

                    if (c % 3 == 0)
                        childGridXY += "Right";
                    else if (c % 3 == 2)
                        childGridXY += "Middle";
                    else
                        childGridXY += "Left";

                    Transform parentTransform = GameObject.Find("Main Grid/Grid Spots/" + parentGridXY + "/Grid Points/" + childGridXY).transform;
                    var spawn = Instantiate(botPrefab);
                    spawn.transform.position = parentTransform.position;
                }
            }
        }
    }

    void updateHighlight(int num)
    {
        Transform parent;
        float yAdj = 0;
        if (num == 0)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Left").transform;
            yAdj = -0.125f;
        }
        else if (num == 1)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Middle").transform;
            yAdj = -0.125f;
        }
        else if (num == 2)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Top Right").transform;
            yAdj = -0.125f;
        }
        else if (num == 3)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Left").transform;
            yAdj = -0.5f;
        }
        else if (num == 4)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Middle").transform;
            yAdj = -0.5f;
        }
        else if (num == 5)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Middle Right").transform;
            yAdj = -0.5f;
        }
        else if (num == 6)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Left").transform;
            yAdj = -0.875f;
        }
        else if (num == 7)
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Middle").transform;
            yAdj = -0.875f;
        }
        else
        {
            parent = GameObject.Find("Main Grid/Grid Spots/Grid Bottom Right").transform;
            yAdj = -0.875f;
        }

        Transform hl = GameObject.Find("Highlight").transform;
        hl.localScale = new Vector3(1, 1, 1);
        hl.position = parent.transform.position;
        hl.position = new Vector3(hl.position.x, hl.position.y + yAdj, hl.position.z);
        foreach (var r in hl.GetComponentsInChildren<Renderer>())
        {
           r.material = playerPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
        }

        if (boardsWon.Contains(num))
        {
            hl.position = new Vector3(0, -8.5f, 0);
            hl.localScale = new Vector3(3, 3, 1);
        }
    }

    void Update()
    {
        if (!gameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    try
                    {
                        if (hit.transform.name.StartsWith("Grid"))
                        {
                            int row = 1; int col = 1;
                            int rowMul = 0; int colMul = 0;
                            float yAdj = -0.125f;
                            var parentGridXY = hit.transform.parent.parent.name.Split(' ');
                            var childGridXY = hit.transform.name.Split(' ');

                            if (parentGridXY[1] == "Middle")
                            {
                                rowMul = 3;
                            }
                            else if (parentGridXY[1] == "Bottom")
                            {
                                rowMul = 6;
                            }

                            if (parentGridXY[2] == "Middle")
                            {
                                colMul = 3;
                            }
                            else if (parentGridXY[2] == "Right")
                            {
                                colMul = 6;
                            }

                            if (childGridXY[1] == "Middle")
                            {
                                row = 2;
                                yAdj = -0.5f;
                            }
                            else if (childGridXY[1] == "Bottom")
                            {
                                row = 3;
                                yAdj = -0.875f;
                            }

                            if (childGridXY[2] == "Middle")
                            {
                                col = 2;
                            }
                            else if (childGridXY[2] == "Right")
                            {
                                col = 3;
                            }

                            row += rowMul;
                            col += colMul;
                            user_input = new Tuple<int, int>(row, col);

                            try
                            {
                                user_move = check_input(state, bot_move);
                            }
                            catch
                            {
                                return;
                            }

                            var spawn = Instantiate(playerPrefab);
                            spawn.transform.position = hit.transform.position;
                            user_state = add_piece(state, user_move, 'X');
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

                            int index = 0;
                            if (hit.transform.name == "Grid Top Left") index = 0;
                            if (hit.transform.name == "Grid Top Middle") index = 1;
                            if (hit.transform.name == "Grid Top Right") index = 2;
                            if (hit.transform.name == "Grid Middle Left") index = 3;
                            if (hit.transform.name == "Grid Middle Middle") index = 4;
                            if (hit.transform.name == "Grid Middle Right") index = 5;
                            if (hit.transform.name == "Grid Bottom Left") index = 6;
                            if (hit.transform.name == "Grid Bottom Middle") index = 7;
                            if (hit.transform.name == "Grid Bottom Right") index = 8;

                            Transform hl = GameObject.Find("Highlight").transform;
                            hl.localScale = new Vector3(1, 1, 1);
                            hl.position = GameObject.Find("Main Grid/Grid Spots/" + hit.transform.name).transform.position;
                            hl.position = new Vector3(hl.position.x, hl.position.y + yAdj, hl.position.z);
                            foreach (var r in hl.GetComponentsInChildren<Renderer>())
                            {
                                r.material = botPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
                            }

                            if (boardsWon.Contains(index))
                            {
                                hl.position = new Vector3(0, -8.5f, 0);
                                hl.localScale = new Vector3(3, 3, 1);
                            }
                            GameObject.Find("Canvas/Action Text").GetComponent<Text>().text = "Blue player is deciding where to play...";
                            inputSuccess = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                        return;
                    }
                }
            }

            if (inputSuccess)
            {
                inputSuccess = false;
                Tuple<string, int> mm = minimax(user_state, user_move, "O", depth);
                string bot_state = mm.Item1;
                bot_move = mm.Item2;
                placeBot(bot_state, state);
                state = bot_state;
                box_won = update_box_won(bot_state);
                List<int> possible_moves = get_possible_moves(new Tuple<int, int>(bot_move, int.MinValue));
                int k = 0; bool seen = false;
                foreach (List<int> b in boardspots)
                {
                    foreach (int i in possible_moves)
                    {
                        if (b.Contains(i))
                        {
                            seen = true;
                            updateHighlight(k);
                            break;
                        }
                    }
                    if (seen) break;
                    k += 1;
                }
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
                GameObject.Find("Canvas/Action Text").GetComponent<Text>().text = "Red player is deciding where to play...";
            }
        }
    }
}
