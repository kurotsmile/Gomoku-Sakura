using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

public class Control : MonoBehaviour {
    public GameObject my_pionblack; // Contient le pion noir
    public GameObject my_pionwhite; // Contient le pion blanc
    public float case_size = 0.40f; // Taille d'une case
    public GameObject my_lueur;
    public GameObject my_lueur_tmp;
    public Sprite blackCross;
	public Sprite whiteCross;
    public Sprite redCross;
    public GameObject panel_result;
    public Text txt_result_win;
    public int player = 1;
    private int other;
    public int playersave = 0;
    public int playerwin;
    public menurules rules;
    public bool b_three = false;
    public bool b_five = false;
    bool isPaused = false;

    public Button ButtonBestslot;

    private int nb_patt = 6;
    private string[] patt = new string[6] { "01110", "010110", "011010", "10110", "11010", "011110" };
    private int nb_pattC = 3;
    private string[] pattC = new string[3] { "0110", "2110", "0112" };

    private Rigidbody2D my_rig; // Notre RigidBody
    private Vector2 position; // Position temporaire
	private SpriteRenderer spriteRenderer; 

    private int[,]          tab = new int[19, 19];
    private GameObject[,]   tabObject = new GameObject[19, 19];
    private int p_x;
    private int p_y;

    private int black_score = 0;
    private int white_score = 0;

    public Text countTextBlack;
    public Text countTextWhite;
    public Text Winner;

    bool isAttack;

    float recalc(float pos) // Calcul et arrondis en fonctions de la position de base du curseur
    {
        float npos = pos;
        if (npos % case_size >= case_size/2)
            npos += case_size - (npos % case_size);
        else if (npos > 0)
            npos -= (npos % case_size);
        else if (npos % case_size <= -case_size/2)
            npos -= case_size + (npos % case_size);
        else
            npos -= (npos % case_size);
        if (npos >= case_size * 9)
            npos = case_size * 9;
        else if (npos <= -case_size * 9)
            npos = -case_size * 9;
        return npos;
    }

    int where_in_tab(float pos) // retourne la case correspondante a une coordonnée x ou y
    {
        float place = pos / case_size;
        if (place > 0)
            place += 9;
        else if (place < 0)
            place = 9 + place;
        else
            place = 9;
        return (int)place;
    }

    int calc_pos(int player) // Verifie l'absence de jeton dans le tableau puis en ajoute un si possible ou retourne le jeton actuelle
	{
        int j;
        int i;

        j = where_in_tab(position.y);
        i = where_in_tab(position.x);
        if (tab[j, i] == 0)
        {
            tab[j, i] = player;
            if (player == 1)
            {
                tabObject[j, i] = Instantiate(my_pionblack, my_rig.position, new Quaternion());
                tabObject[j, i].transform.SetParent(this.transform);
                GameObject.Find("Game").GetComponent<Game_handle>().play_sound(2);
            }
            else
            {
                tabObject[j, i] = Instantiate(my_pionwhite, my_rig.position, new Quaternion());
                tabObject[j, i].transform.SetParent(this.transform);
                GameObject.Find("Game").GetComponent<Game_handle>().play_sound(3);
            }
            VerifCaptured(j, i);
            return (0);
        }
        return (tab[j,i]);
    }

    int verif_line()
    {
        int i = 0, j;
        int stack = 0;
        int flag = 0;
        while (i < 19)
        {	
            j = 0;
            while (j < 19)
            {
                if (tab[i,j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5 && (b_five == false || GetFive(0, j, i, tab[i, j]) == 0))
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                j++;
            }
            i++;
        }
        return (0);
    }

    int verif_col()
    {
        int i, j=0;
        int stack = 0;
        int flag = 0;
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5 && (b_five == false || GetFive(2, j, i, tab[i, j]) == 0))
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i++;
            }
            j++;
        }
        return (0);
    }

    int verif_diag1()
    {
        int i_s = 4, j_s = 0;
        int flag = 0, stack = 0;
        int i, j;
        while (j_s < 19)
        {
            i = i_s;
            j = j_s;
            while (i >= 0 && j <= 18)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5 && (b_five == false || GetFive(4, j, i, tab[i, j]) == 0))
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i--;
                j++;
            }
            stack = 0;
            flag = 0;
            if (i_s < 18)
                i_s++;
            else
                j_s++;
        }
        return (0);
    }

    int verif_diag2()
    {
        int i_s = 4, j_s = 18;
        int flag = 0, stack = 0;
        int i, j;
        while (j_s > 0)
        {
            i = i_s;
            j = j_s;
            while (i >= 0 && j >= 0)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5 && (b_five == false || GetFive(6, j, i, tab[i, j]) == 0))
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i--;
                j--;
            }
            stack = 0;
            flag = 0;
            if (i_s < 18)
                i_s++;
            else
                j_s--;
        }
        return (0);
    }

    int verif_win()
    {
        int res;
        if ((res = verif_line()) != 0
            || (res = verif_col()) != 0
            || (res = verif_diag1()) != 0
            || (res = verif_diag2()) != 0
            || black_score >= 10 || white_score >= 10)
        {
            res = player;
            playerwin = player;
            isPaused = true;
            Cursor.visible = true;
            this.panel_result.SetActive(true);
            this.txt_result_win.text = "Player " + res + " wins!";
            GameObject.Find("Game").GetComponent<Game_handle>().show_result_game();
            return (res);
        }
        return (0);
    }

    public void Start () {
        Cursor.visible = true;

        player = UnityEngine.Random.Range(1, 3);
        spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer.sprite == null)
			spriteRenderer.sprite = blackCross;

        countTextBlack.text = "0/10";
        countTextWhite.text = "0/10";
    }

    private void     VerifCaptured(int j, int i)
    {
        int eaten;

        eaten = (player == 1 ? 2 : 1);

        VerifCapturedLineVertical(j, i, eaten);
        VerifCapturedLineHorizontal(j, i, eaten);
        VerifCapturedDiagonal(j, i, eaten);
        VerifCapturedDiagonal2(j, i, eaten);
        UpdateScoreText();
    }

    private void    VerifCapturedLineHorizontal(int j, int i, int eaten) {
        // Check at left of pawn
        if (i >= 3 && tab[j, i - 3] == player && tab[j, i - 2] == eaten && tab[j, i - 1] == eaten)
        {
            // Clean both arrays
            tab[j, i - 2] = 0;
            tab[j, i - 1] = 0;
            Destroy(tabObject[j, i - 2]);
            Destroy(tabObject[j, i - 1]);

            AddScore();
        }

        // Check at right of pawn
        if (i <= 15 && tab[j, i + 3] == player && tab[j, i + 1] == eaten && tab[j, i + 2] == eaten)
        {
            // Clean both arrays
            tab[j, i + 1] = 0;
            tab[j, i + 2] = 0;
            Destroy(tabObject[j, i + 1]);
            Destroy(tabObject[j, i + 2]);

            AddScore();
        }
    }

    private void VerifCapturedLineVertical(int j, int i, int eaten)
    {
        // Check at bot of pawn
        if (j >= 3 && tab[j - 3, i] == player && tab[j - 2, i] == eaten && tab[j - 1, i] == eaten)
        {
            // Clean both arrays
            tab[j - 2, i] = 0;
            tab[j - 1, i] = 0;
            Destroy(tabObject[j - 2, i]);
            Destroy(tabObject[j - 1, i]);

            AddScore();
        }

        // Check at top of pawn
        if (j <= 15 && tab[j + 3, i] == player && tab[j + 1, i] == eaten && tab[j + 2, i] == eaten)
        {
            // Clean both arrays
            tab[j + 1, i] = 0;
            tab[j + 2, i] = 0;
            Destroy(tabObject[j + 1, i]);
            Destroy(tabObject[j + 2, i]);

            AddScore();
        }
    }

    /* Check:
    **              X
    **          O  
    **      O
    **  X
    */
    private void    VerifCapturedDiagonal(int j, int i, int eaten)
    {
        if (j >= 3 && i >= 3 && tab[j - 3, i - 3] == player && tab[j - 2, i - 2] == eaten && tab[j - 1, i - 1] == eaten)
        {
            tab[j - 1, i - 1] = 0;
            tab[j - 2, i - 2] = 0;
            Destroy(tabObject[j - 1, i - 1]);
            Destroy(tabObject[j - 2, i - 2]);

            AddScore();
        }

        if (j <= 15 && i <= 15 && tab[j + 3, i + 3] == player && tab[j + 2, i + 2] == eaten && tab[j + 1, i + 1] == eaten)
        {
            tab[j + 1, i + 1] = 0;
            tab[j + 2, i + 2] = 0;
            Destroy(tabObject[j + 1, i + 1]);
            Destroy(tabObject[j + 2, i + 2]);

            AddScore();
        }
    }


    /* Check:
    **      X       
    **          O  
    **              O
    **                  X
    */
    private void VerifCapturedDiagonal2(int j, int i, int eaten)
    {
        if (j <= 15 && i >= 3 && tab[j + 3, i - 3] == player && tab[j + 2, i - 2] == eaten && tab[j + 1, i - 1] == eaten)
        {
            tab[j + 1, i - 1] = 0;
            tab[j + 2, i - 2] = 0;
            Destroy(tabObject[j + 1, i - 1]);
            Destroy(tabObject[j + 2, i - 2]);

            AddScore();
        }

        if (j >= 3 && i <= 15 && tab[j - 3, i + 3] == player && tab[j - 2, i + 2] == eaten && tab[j - 1, i + 1] == eaten)
        {
            tab[j - 1, i + 1] = 0;
            tab[j - 2, i + 2] = 0;
            Destroy(tabObject[j - 1, i + 1]);
            Destroy(tabObject[j - 2, i + 2]);

            AddScore();
        }
    }

    private void AddScore()
    {
        if (player == 1)
            black_score += 2;
        else
            white_score += 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (my_rig == null)
            my_rig = GetComponent<Rigidbody2D>();
        position = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        position.x = recalc(position.x);
        position.y = recalc(position.y);
        my_rig.position = position;
        CheckSelectorSprite();
        playersave = player;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.find_pos();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            rules.gameObject.SetActive(true);
        }

        if (isPaused == false)
        {
            rules.gameObject.SetActive(false);
            Time.timeScale = 1;
            if (Input.GetMouseButtonDown(0)) {
                if (b_three == false || IsThree(position.x, position.y) < 2) {
                    if (calc_pos(player) == 0) {
                        verif_win();
                        if (my_lueur_tmp != null)
                            Destroy(my_lueur_tmp);
                        player = (player == 1 ? 2 : 1);
                        other = (player == 1 ? 2 : 1);
                    }
                }
            }
        }
        else
           Time.timeScale = 0;
    }

    public void Checkpaused() {
        if (isPaused == true)
            isPaused = false;
        else
            isPaused = true;
    }

    void CheckSelectorSprite() {
		int j;
		int i;

		j = where_in_tab(position.y);
		i = where_in_tab(position.x);
        if (isPaused)
            spriteRenderer.sprite = null;
        else if (tab[j, i] != 0)
            spriteRenderer.sprite = redCross;
        else if (player == 1)
            spriteRenderer.sprite = blackCross;
        else
            spriteRenderer.sprite = whiteCross;
	}

    private void    UpdateScoreText()
    {
        countTextBlack.text = black_score.ToString() + "/10";
        countTextWhite.text = white_score.ToString() + "/10";
    }


    // REGLE DE TROIS
    //
    //
    // Cherche les differents pattern
    int DetectPatt(string tab)
    {
        int i = 0;

        while (i < this.nb_patt)
        {
            if (tab.Contains(this.patt[i]))
            {
                print("Double trois trouvé! -> " + tab);
                return (1);
            }
            i++;
        }
        return 0;
    }

    // conversion ligne
    int ThreeInLine(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = y - 3;
        int i = 0;

        while (j < y + 4 && j < 19)
        {
            if (j >= 0 && this.tab[x, j] == player || j == y)
                tmptab[i] = '1';
            else if (j >= 0 && this.tab[x, j] == 0)
                tmptab[i] = '0';
            else
                tmptab[i] = '2';
            i++;
            j++;
        }
        while (i < 7)
            tmptab[i++] = '2';
        string res = new string(tmptab);
        print("Line -> " + res);
        return DetectPatt(new string(tmptab));
    }

    // conversion colonne
    int ThreeInCol(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x - 3;
        int i = 0;

        while (j < x + 4 && j < 19)
        {
            if (j >= 0 && this.tab[j, y] == player || j == x)
                tmptab[i] = '1';
            else if (j >= 0 && this.tab[j, y] == 0)
                tmptab[i] = '0';
            else
                tmptab[i] = '2';
            i++;
            j++;
        }
        while (i < 7)
            tmptab[i++] = '2';
        string res = new string(tmptab);
        print("Colonne -> " + res);
        return DetectPatt(new string(tmptab));
    }

    //converstion diagonale
    int ThreeInDiag(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x;
        int i = y;
        int compt = 0;

        while (compt < 3 && j > 0 && i > 0)
        {
            j--;
            i--;
            compt++;
        }
        compt = 0;
        while (j < x + 4 && i < y + 4 && j < 19 && i < 19)
        {
            if (this.tab[j, i] == player || (j == x && i == y))
                tmptab[compt] = '1';
            else if (this.tab[j, i] == 0)
                tmptab[compt] = '0';
            else
                tmptab[compt] = '2';
            i++;
            j++;
            compt++;
        }
        while (compt < 7)
            tmptab[compt++] = '2';
        string res = new string(tmptab);
        print("Diag -> " + res);
        return DetectPatt(new string(tmptab));
    }

    //converstion diagonale2
    int ThreeInDiag2(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x;
        int i = y;
        int compt = 0;

        while (compt < 3 && j > 0 && i < 18)
        {
            j--;
            i++;
            compt++;
        }
        compt = 0;
        while (j < x + 4 && i > y - 4 && j < 19 && i >= 0)
        {
            if (this.tab[j, i] == player || (j == x && i == y))
                tmptab[compt] = '1';
            else if (this.tab[j, i] == 0)
                tmptab[compt] = '0';
            else
                tmptab[compt] = '2';
            i--;
            j++;
            compt++;
        }
        while (compt < 7)
            tmptab[compt++] = '2';
        string res = new string(tmptab);
        print("Diag2 -> " + res);
        return DetectPatt(new string(tmptab));
    }

    int IsThree(float a, float b)
    {
        int res = 0;
        int x = where_in_tab(b);
        int y = where_in_tab(a);

        res = ThreeInLine(x, y);
        res = res + ThreeInCol(x, y);
        res = res + ThreeInDiag(x, y);
        res = res + ThreeInDiag2(x, y);
        print("Total -> " + res);
        return res;
    }

    int findMove()
    {
        if (isAttack == false) // boucle défensive
        {
            if (IA_CanWin() ||          // Si on peut gagner en une fois, poser le pion de la win
                IA_ennemyCanWin() ||    // Si on peut perdre en une fois, poser le pion pour empêcher la win adverse
                IA_blockEnnemy3() ||    // Si l'ennemi a un alignement de 3, on bloque
                IA_canEatEnnemy())      // Si on peut manger
                return 0;

            // On a rien joué, donc on passe en mode attaque et on rappelle findMove();
            // Décommenter pour tester partie offensive    

            isAttack = true;
            findMove();
            print("Rien trouvé, on passe en mode aggro");
        }
        else // boucle offensive
        {
            if (IA_completeAlign3() ||      // Complète les alignements de 3 pions
                IA_completeAlign2() ||      // Complète les alignements de 2 pions
                IA_randomNear() ||          // Pose un deuxième jeton
                IA_recursiveRandom(5, 12))  // Random le plus près du milieu
                isAttack = false;
            return 0;
        }
        return 0;
    }

    bool IA_Play(int x, int y)
    {
        print("IA conseille de jouer en [" + y + "] [" + x + "]");

        if (my_lueur_tmp == null)
        {
            position.x = (case_size * (x - 9));
            position.y = (case_size * (y - 9));
            my_rig.position = position;
            my_lueur_tmp = Instantiate(my_lueur, my_rig.position, new Quaternion());
        }
        return true;
    }

    bool IA_CanWin()
    {
        int j = 0;
        int i;

        print("Try IA_CanWin");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == player)
                {
                    if (i < 16 && tab[j, i + 1] == player && tab[j, i + 2] == player && tab[j, i + 3] == player) // horizontal  O X X X X O 
                    {
                        if (i > 0 && tab[j, i - 1] == 0)
                            return IA_Play(i - 1, j);
                        else if (i < 15 && tab[j, i + 4] == 0)
                            return IA_Play(i + 4, j);
                    }
                    else if (i < 15 && tab[j, i + 1] == 0 && tab[j, i + 2] == player && tab[j, i + 3] == player && tab[j, i + 4] == player) // X O X X X
                        return IA_Play(i + 1, j);
                    else if (i < 15 && tab[j, i + 1] == player && tab[j, i + 2] == 0 && tab[j, i + 3] == player && tab[j, i + 4] == player) // X X O X X
                        return IA_Play(i + 2, j);
                    else if (i < 15 && tab[j, i + 1] == player && tab[j, i + 2] == player && tab[j, i + 3] == 0 && tab[j, i + 4] == player) // X X X O X
                        return IA_Play(i + 3, j);

                    else if (j < 16 && tab[j + 1, i] == player && tab[j + 2, i] == player && tab[j + 3, i] == player) // vertical
                    {
                        if (j > 0 && tab[j - 1, i] == 0)
                            return IA_Play(i, j - 1);
                        else if (j < 15 && tab[j + 4, i] == 0)
                            return IA_Play(i, j + 4);
                    }
                    else if (j < 15 && tab[j + 1, i] == 0 && tab[j + 2, i] == player && tab[j + 3, i] == player && tab[j + 4, i] == player)
                        return IA_Play(i, j + 1);
                    else if (j < 15 && tab[j + 1, i] == player && tab[j + 2, i] == 0 && tab[j + 3, i] == player && tab[j + 4, i] == player)
                        return IA_Play(i, j + 2);
                    else if (j < 15 && tab[j + 1, i] == player && tab[j + 2, i] == player && tab[j + 3, i] == 0 && tab[j + 4, i] == player)
                        return IA_Play(i, j + 3);

                    else if (i > 4 && j < 16 && tab[j + 1, i - 1] == player && tab[j + 2, i - 2] == player && tab[j + 3, i - 3] == player) // diago no se
                    {
                        if (j > 0 && i < 18 && tab[j - 1, i + 1] == 0)
                            return IA_Play(i + 1, j - 1);
                        else if (j < 15 && i > 3 && tab[j + 4, i - 4] == 0)
                            return IA_Play(i - 4, j + 4);
                    }
                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == 0 && tab[j + 2, i - 2] == player && tab[j + 3, i - 3] == player && tab[j + 4, i - 4] == player)
                        return IA_Play(i - 1, j + 1);
                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == player && tab[j + 2, i - 2] == 0 && tab[j + 3, i - 3] == player && tab[j + 4, i - 4] == player)
                        return IA_Play(i - 2, j + 2);
                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == player && tab[j + 2, i - 2] == player && tab[j + 3, i - 3] == 0 && tab[j + 4, i - 4] == player)
                        return IA_Play(i - 3, j + 3);

                    else if (i < 16 && j < 16 && tab[j + 1, i + 1] == player && tab[j + 2, i + 2] == player && tab[j + 3, i + 3] == player) // diago ne so
                    {
                        if (j > 0 && i > 0 && tab[j - 1, i - 1] == 0)
                            return IA_Play(i - 1, j - 1);
                        else if (j < 15 && i < 15 && tab[j + 4, i + 4] == 0)
                            return IA_Play(i + 4, j + 4);
                    }
                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == 0 && tab[j + 2, i + 2] == player && tab[j + 3, i + 3] == player && tab[j + 4, i + 4] == player)
                        return IA_Play(i + 1, j + 1);
                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == player && tab[j + 2, i + 2] == 0 && tab[j + 3, i + 3] == player && tab[j + 4, i + 4] == player)
                        return IA_Play(i + 2, j + 2);
                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == player && tab[j + 2, i + 2] == player && tab[j + 3, i + 3] == 0 && tab[j + 4, i + 4] == player)
                        return IA_Play(i + 3, j + 3);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_ennemyCanWin()
    {
        int j = 0;
        int i;

        print("Try IA_ennemyCanWin");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == other)
                {
                    if (i < 15 && tab[j, i + 1] == 0 && tab[j, i + 2] == other && tab[j, i + 3] == other && tab[j, i + 4] == other) // X O X X X
                        return IA_Play(i + 1, j);
                    else if (i < 15 && tab[j, i + 1] == other && tab[j, i + 2] == 0 && tab[j, i + 3] == other && tab[j, i + 4] == other) // X X O X X
                        return IA_Play(i + 2, j);
                    else if (i < 15 && tab[j, i + 1] == other && tab[j, i + 2] == other && tab[j, i + 3] == 0 && tab[j, i + 4] == other) // X X X O X
                        return IA_Play(i + 3, j);

                    else if (j < 15 && tab[j + 1, i] == 0 && tab[j + 2, i] == other && tab[j + 3, i] == other && tab[j + 4, i] == other)
                        return IA_Play(i, j + 1);
                    else if (j < 15 && tab[j + 1, i] == other && tab[j + 2, i] == 0 && tab[j + 3, i] == other && tab[j + 4, i] == other)
                        return IA_Play(i, j + 2);
                    else if (j < 15 && tab[j + 1, i] == other && tab[j + 2, i] == other && tab[j + 3, i] == 0 && tab[j + 4, i] == other)
                        return IA_Play(i, j + 3);

                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == 0 && tab[j + 2, i - 2] == other && tab[j + 3, i - 3] == other && tab[j + 4, i - 4] == other)
                        return IA_Play(i - 1, j + 1);
                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == other && tab[j + 2, i - 2] == 0 && tab[j + 3, i - 3] == other && tab[j + 4, i - 4] == other)
                        return IA_Play(i - 2, j + 2);
                    else if (i > 3 && j < 15 && tab[j + 1, i - 1] == other && tab[j + 2, i - 2] == other && tab[j + 3, i - 3] == 0 && tab[j + 4, i - 4] == other)
                        return IA_Play(i - 3, j + 3);

                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == 0 && tab[j + 2, i + 2] == other && tab[j + 3, i + 3] == other && tab[j + 4, i + 4] == other)
                        return IA_Play(i + 1, j + 1);
                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == other && tab[j + 2, i + 2] == 0 && tab[j + 3, i + 3] == other && tab[j + 4, i + 4] == other)
                        return IA_Play(i + 2, j + 2);
                    else if (i < 15 && j < 15 && tab[j + 1, i + 1] == other && tab[j + 2, i + 2] == other && tab[j + 3, i + 3] == 0 && tab[j + 4, i + 4] == other)
                        return IA_Play(i + 3, j + 3);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_blockEnnemy3()
    {
        int j = 0;
        int i;

        print("Try IA_blockEnnemy3");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == other)
                {
                    // horizontal
                    if (i < 17 && tab[j, i + 1] == other && tab[j, i + 2] == other)
                    {
                        if (i > 0 && tab[j, i - 1] == 0)
                            return IA_Play(i - 1, j);
                        else if (i < 16 && tab[j, i + 3] == 0)
                            return IA_Play(i + 3, j);
                    }
                    else if (i < 16 && tab[j, i + 1] == 0 && tab[j, i + 2] == other && tab[j, i + 3] == other)
                        return IA_Play(i + 1, j);
                    else if (i < 16 && tab[j, i + 1] == other && tab[j, i + 2] == 0 && tab[j, i + 3] == other)
                        return IA_Play(i + 2, j);

                    // vertical
                    else if (j < 17 && tab[j + 1, i] == other && tab[j + 2, i] == other)
                    {
                        if (j > 0 && tab[j - 1, i] == 0)
                            return IA_Play(i, j - 1);
                        else if (j < 16 && tab[j + 3, i] == 0)
                            return IA_Play(i, j + 3);
                    }
                    else if (j < 16 && tab[j + 1, i] == 0 && tab[j + 2, i] == other && tab[j + 3, i] == other)
                        return IA_Play(i, j + 1);
                    else if (j < 16 && tab[j + 1, i] == other && tab[j + 2, i] == 0 && tab[j + 3, i] == other)
                        return IA_Play(i, j + 2);

                    // diago ne so
                    else if (i < 17 && j < 17 && tab[j + 1, i + 1] == other && tab[j + 2, i + 2] == other)
                    {
                        if (j > 0 && i > 0 && tab[j - 1, i - 1] == 0)
                            return IA_Play(i - 1, j - 1);
                        else if (j < 16 && i < 16 && tab[j + 3, i + 3] == 0)
                            return IA_Play(i + 3, j + 3);
                    }
                    else if (i < 16 && j < 16 && tab[j + 1, i + 1] == 0 && tab[j + 2, i + 2] == other && tab[j + 3, i + 3] == other)
                        return IA_Play(i + 1, j + 1);
                    else if (i < 16 && j < 16 && tab[j + 1, i + 1] == other && tab[j + 2, i + 2] == 0 && tab[j + 3, i + 3] == other)
                        return IA_Play(i + 2, j + 2);

                    // diago no se
                    else if (j < 17 && i > 1 && tab[j + 1, i - 1] == other && tab[j + 2, i - 2] == other)
                    {
                        if (j > 0 && i < 18 && tab[j - 1, i + 1] == 0)
                            return IA_Play(i + 1, j - 1);
                        else if (i > 2 && j < 16 && tab[j + 3, i - 3] == 0)
                            return IA_Play(i - 3, j + 3);
                    }
                    else if (i > 3 && j < 16 && tab[j + 1, i - 1] == 0 && tab[j + 2, i - 2] == other && tab[j + 3, i - 3] == other)
                        return IA_Play(i - 1, j + 1);
                    else if (i > 3 && j < 16 && tab[j + 1, i - 1] == other && tab[j + 2, i - 2] == 0 && tab[j + 3, i - 3] == other)
                        return IA_Play(i - 2, j + 2);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_canEatEnnemy()
    {
        int j = 0;
        int i;

        print("Try IA_canEatEnnemy");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == player)
                {
                    // Horizontal
                    if (i > 2 && tab[j, i - 3] == 0 && tab[j, i - 2] == other && tab[j, i - 1] == other)
                        return IA_Play(i - 3, j);
                    else if (i < 16 && tab[j, i + 3] == 0 && tab[j, i + 2] == other && tab[j, i + 1] == other)
                        return IA_Play(i + 3, j);

                    // Vertical
                    else if (j > 2 && tab[j - 3, i] == 0 && tab[j - 2, i] == other && tab[j - 1, i] == other)
                        return IA_Play(i, j - 3);
                    else if (j < 16 && tab[j + 3, i] == 0 && tab[j + 2, i] == other && tab[j + 1, i] == other)
                        return IA_Play(i, j + 3);

                    // Diagos
                    else if (i > 2 && j < 16 && tab[j + 3, i - 3] == 0 && tab[j + 2, i - 2] == other && tab[j + 1, i - 1] == other)
                        return IA_Play(i - 3, j + 3);

                    else if (i > 2 && j > 2 && tab[j - 3, i - 3] == 0 && tab[j - 2, i - 2] == other && tab[j - 1, i - 1] == other)
                        return IA_Play(i - 3, j - 3);

                    else if (i < 16 && j < 16 && tab[j + 3, i + 3] == 0 && tab[j + 2, i + 2] == other && tab[j + 1, i + 1] == other)
                        return IA_Play(i + 3, j + 3);

                    else if (i < 16 && j > 2 && tab[j - 3, i + 3] == 0 && tab[j - 2, i + 2] == other && tab[j - 1, i + 1] == other)
                        return IA_Play(i + 3, j - 3);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_completeAlign3()
    {
        int j = 0;
        int i;

        print("Try IA_completeAlign3");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == player)
                {
                    // Horizontal
                    if (i < 17 && tab[j, i + 1] == player && tab[j, i + 2] == player)
                    {
                        if (i > 0 && tab[j, i - 1] == 0)
                            return IA_Play(i - 1, j);
                        else if (i < 16 && tab[j, i + 3] == 0)
                            return IA_Play(i + 3, j);
                    }
                    else if (i < 16 && tab[j, i + 1] == 0 && tab[j, i + 2] == player && tab[j, i + 3] == player) // X O X X
                        return IA_Play(i + 1, j);
                    else if (i < 16 && tab[j, i + 1] == player && tab[j, i + 2] == 0 && tab[j, i + 3] == player) // X X O X
                        return IA_Play(i + 2, j);

                    // Vertical
                    else if (j < 17 && tab[j + 1, i] == player && tab[j + 2, i] == player)
                    {
                        if (j > 0 && tab[j - 1, i] == 0)
                            return IA_Play(i, j - 1);
                        else if (j < 16 && tab[j + 3, i] == 0)
                            return IA_Play(i, j + 3);
                    }
                    else if (j < 16 && tab[j + 1, i] == 0 && tab[j + 2, i] == player && tab[j + 3, i] == player) // X O X X
                        return IA_Play(i, j + 1);
                    else if (j < 16 && tab[j + 1, i] == player && tab[j + 2, i] == 0 && tab[j + 3, i] == player) // X X O X
                        return IA_Play(i, j + 2);

                    // Diag so ne
                    else if (i < 17 && j < 17 && tab[j + 1, i + 1] == player && tab[j + 2, i + 2] == player)
                    {
                        if (j > 0 && i > 0 && tab[j - 1, i - 1] == 0)
                            return IA_Play(i - 1, j - 1);
                        else if (i < 16 && j < 16 && tab[j + 3, i + 3] == 0)
                            return IA_Play(i + 3, j + 3);
                    }
                    else if (j < 16 && i < 16 && tab[j + 1, i + 1] == 0 && tab[j + 2, i + 2] == player && tab[j + 3, i + 3] == player) // X O X X
                        return IA_Play(i + 1, j + 1);
                    else if (j < 16 && i < 16 && tab[j + 1, i + 1] == player && tab[j + 2, i + 2] == 0 && tab[j + 3, i + 3] == player) // X X O X
                        return IA_Play(i + 1, j + 2);

                    // Diag se no
                    else if (i < 17 && j < 17 && tab[j + 1, i - 1] == player && tab[j + 2, i - 2] == player)
                    {
                        if (j > 0 && i < 18 && tab[j - 1, i + 1] == 0)
                            return IA_Play(i + 1, j - 1);
                        else if (j < 16 && i > 2 && tab[j + 3, i - 3] == 0)
                            return IA_Play(i - 3, j + 3);
                    }
                    else if (j < 16 && i < 16 && tab[j + 1, i - 1] == 0 && tab[j + 2, i - 2] == player && tab[j + 3, i - 3] == player) // X O X X
                        return IA_Play(i - 1, j + 1);
                    else if (j < 16 && i < 16 && tab[j + 1, i - 1] == player && tab[j + 2, i - 2] == 0 && tab[j + 3, i - 3] == player) // X X O X
                        return IA_Play(i - 2, j + 2);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_completeAlign2()
    {
        int j = 0;
        int i;

        print("Try IA_completeAlign2");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == player)
                {
                    // Horizontal
                    if (i < 18 && tab[j, i + 1] == player)
                    {
                        print("Pose horizontal");
                        if (i > 0 && tab[j, i - 1] == 0)
                            return IA_Play(i - 1, j);
                        else if (i < 17 && tab[j, i + 2] == 0)
                            return IA_Play(i + 2, j);
                    }
                    // Vertical
                    else if (j < 18 && tab[j + 1, i] == player)
                    {
                        print("Pose vertical");
                        if (j > 0 && tab[j - 1, i] == 0)
                            return IA_Play(i, j - 1);
                        else if (j < 17 && tab[j + 2, i] == 0)
                            return IA_Play(i, j + 2);
                    }
                    // Diag so ne
                    else if (i < 18 && j < 18 && tab[j + 1, i + 1] == player)
                    {
                        print("Pose so ne");
                        if (j > 0 && i > 0 && tab[j - 1, i - 1] == 0)
                            return IA_Play(i - 1, j - 1);
                        else if (i < 17 && j < 17 && tab[j + 2, i + 2] == 0)
                            return IA_Play(i + 2, j + 2);
                    }
                    // Diag se no
                    else if (i > 0 && j < 18 && tab[j + 1, i - 1] == player)
                    {
                        print("Pose se no");
                        if (j > 0 && i < 18 && tab[j - 1, i + 1] == 0)
                            return IA_Play(i + 1, j - 1);
                        else if (j < 17 && i > 1 && tab[j + 2, i - 2] == 0)
                            return IA_Play(i - 2, j + 2);
                    }
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    bool IA_randomNear()
    {
        int j = 0;
        int i;

        print("Try IA_randomNear");
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == player)
                {
                    if (i < 18 && tab[j, i + 1] == 0)
                        return IA_Play(i + 1, j);
                    else if (i > 0 && tab[j, i - 1] == 0)
                        return IA_Play(i - 1, j);

                    else if (j < 18 && tab[j + 1, i] == 0)
                        return IA_Play(i, j + 1);
                    else if (j > 0 && tab[j - 1, i] == 0)
                        return IA_Play(i, j - 1);

                    else if (i < 18 && j < 18 && tab[j + 1, i + 1] == 0)
                        return IA_Play(i + 1, j + 1);
                    else if (i > 0 && j > 0 && tab[j - 1, i - 1] == 0)
                        return IA_Play(i - 1, j - 1);

                    else if (j > 0 && i < 18 && tab[j - 1, i + 1] == 0)
                        return IA_Play(i + 1, j - 1);
                    else if (j < 18 && i > 0 && tab[j + 1, i - 1] == 0)
                        return IA_Play(i - 1, j + 1);
                }
                i++;
            }
            j++;
        }
        return (false);
    }

    // recursive random
    bool IA_recursiveRandom(int a, int b)
    {
        int i, j;

        print("Try IA_recursiveRandom");

        i = UnityEngine.Random.Range(a, b);
        j = UnityEngine.Random.Range(a, b);

        if (i < 0)
            i = 0;
        if (j > 18)
            j = 18;

        if (tab[j, i] == 0)
            return IA_Play(i, j);
        else
            return IA_recursiveRandom(a--, b++);

    }

    // REGLE DE CINQ
    // Regle de Cinq
    // Appel a GetFive dans les verif_win a 5 stacks
    // 0 up
    // 2 left
    // 4 diag1
    // 6 diag2
    int GetFive(int dir, int x, int y, int player)
    {
        char[] tab = new char[5];
        int i = 0;

        while (i < 5)
        {
            print(this.tab[y, x] + "-> [" + y + "] - [" + x + "]");
            if (IsCapturable(y, x, player) != 0)
            {
                return (1);
            }
            if (dir == 0)
                x--;
            else if (dir == 2)
                y--;
            else if (dir == 4)
            {
                x--;
                y++;
            }
            else if (dir == 6)
            {
                x++;
                y++;
            }
            i++;
        }
        print("5 non prenable!");
        return (0);

    }

    int CaptPatt(string tab)
    {
        int i = 0;

        while (i < this.nb_pattC)
        {
            if (tab.Contains(this.pattC[i]))
            {
                print("Prenable!" + tab + " en " + pattC[i]);
                return (1);
            }
            i++;
        }
        return 0;
    }

    int IsCapturable(int y, int x, int player)
    {
        string test;
        char[] tmptab = new char[5];
        int i = y, j = x;
        int comp = 0;

        //Check ligne
        if (x < 18 && x > 1)
        {
            i = y; j = x - 2;
            while (comp < 5 && j < 19)
            {
                tmptab[comp++] = convChar(tab[i, j++], player);
            }
            test = new string(tmptab);
            print("Ligne -> " + test);
            if (CaptPatt(new string(tmptab)) != 0)
                return 1;
            comp = 0;
        }
        //check colon
        if (y < 18 && y > 1)
        {
            j = x; i = y - 2;
            while (comp < 5 && i < 19)
            {
                tmptab[comp++] = convChar(tab[i++, j], player);
            }
            test = new string(tmptab);
            print("Colon -> " + test);
            if (CaptPatt(new string(tmptab)) != 0)
                return 1;
            comp = 0;
        }
        //check diag1
        if (y < 18 && y > 1 && x < 18 && x > 1)
        {
            i = y - 2; j = x - 2;
            while (comp < 5 && i < 19 && j < 19)
            {
                tmptab[comp++] = convChar(tab[i++, j++], player);
            }
            test = new string(tmptab);
            print("Diag1 -> " + test);
            if (CaptPatt(new string(tmptab)) != 0)
                return 1;
            comp = 0;
            //diag2
            i = y - 2; j = x + 2;
            while (comp < 5 && i < 19 && j > 0)
            {
                tmptab[comp++] = convChar(tab[i++, j--], player);
            }
            test = new string(tmptab);
            print("Diag2 -> " + test);
            if (CaptPatt(new string(tmptab)) != 0)
                return 1;
        }
        return (0);
    }

    char convChar(int token, int play) // Retourne un caractère en fonction du player
    {
        if (token == play)
            return ('1');
        else if (token == 0)
            return ('0');
        return ('2');
    }

    public void game_reset()
    {
        this.panel_result.SetActive(false);
        this.tabObject = new GameObject[19, 19];
        this.tab = new int[19, 19];
        this.black_score = 0;
        this.white_score = 0;
        this.playerwin = 0;
        countTextBlack.text = "0/10";
        countTextWhite.text = "0/10";
        this.isPaused = false;
        foreach (Transform tr in this.transform) Destroy(tr.gameObject);
    }

    public int black_scores()
    {
        return this.black_score;
    }

    public void find_pos()
    {
        DateTime before = DateTime.Now;
        findMove();
        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("RéactivitéIA: " + duration.Milliseconds + "ms");
    }
}
