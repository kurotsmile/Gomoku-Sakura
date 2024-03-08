using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_handle : MonoBehaviour
{
    public Carrot.Carrot carrot;
    [Header("Game Obj")]
    public ControlIA control_p_vs_ai;
    public Control control_p_vs_p;
    public GameObject obj_effect_sakura;

    [Header("Panel Game")]
    public GameObject panel_menu;
    public GameObject panel_play;
    public Text txt_panel_title_play;
    public Text txt_total_score;

    [Header("Sound")]
    public AudioSource[] sound;

    private bool is_play_p_vs_p = false;
    private int game_total_score = 0;

    void Start()
    {
        this.carrot.Load_Carrot(this.check_exit_game);
        this.carrot.act_after_delete_all_data=this.Start;
        this.panel_menu.SetActive(true);
        this.panel_play.SetActive(false);

        this.control_p_vs_ai.gameObject.SetActive(false);
        this.control_p_vs_p.gameObject.SetActive(false);

        this.game_total_score = PlayerPrefs.GetInt("game_total_score",0);
        this.txt_total_score.text = this.game_total_score.ToString();
        this.obj_effect_sakura.SetActive(true);

        if (this.carrot.get_status_sound()) this.carrot.game.load_bk_music(this.sound[0]);
    }

    private void check_exit_game()
    {
        if (this.panel_play.activeInHierarchy)
        {
            this.btn_back_home();
            this.carrot.set_no_check_exit_app();
        }
    }

    public void btn_play_two_game()
    {
        this.carrot.play_sound_click();
        this.control_p_vs_ai.gameObject.SetActive(false);
        this.control_p_vs_p.gameObject.SetActive(true);
        this.control_p_vs_p.game_reset();
        this.control_p_vs_ai.game_reset();
        this.panel_play.SetActive(true);
        this.panel_menu.SetActive(false);
        this.is_play_p_vs_p = true;
        this.txt_panel_title_play.text = "Player vs Player";
        this.obj_effect_sakura.SetActive(false);
    }

    public void btn_play_ai_game()
    {
        this.carrot.play_sound_click();
        this.control_p_vs_ai.gameObject.SetActive(true);
        this.control_p_vs_p.gameObject.SetActive(false);
        this.control_p_vs_p.game_reset();
        this.control_p_vs_ai.game_reset();
        this.panel_play.SetActive(true);
        this.panel_menu.SetActive(false);
        this.is_play_p_vs_p = false;
        this.txt_panel_title_play.text = "Player vs AI";
        this.obj_effect_sakura.SetActive(false);
    }

    public void btn_replay()
    {
        this.carrot.ads.show_ads_Interstitial();
        if (this.is_play_p_vs_p)
            this.btn_play_two_game();
        else
            this.btn_play_ai_game();
        this.control_p_vs_ai.game_reset();
    }

    public void btn_show_setting()
    {
        this.carrot.ads.show_ads_Interstitial();
        if (this.panel_play.activeInHierarchy)
        {
            if (this.is_play_p_vs_p)
                this.control_p_vs_p.gameObject.SetActive(false);
            else
                this.control_p_vs_ai.gameObject.SetActive(false);
        }
        Carrot.Carrot_Box box_setting=this.carrot.Create_Setting();
        box_setting.set_act_before_closing(this.after_close_setting);
    }

    private void after_close_setting(List<string> list_change)
    {
        this.carrot.ads.show_ads_Interstitial();
        foreach (string s in list_change)
        {
            if (s == "list_bk_music") this.carrot.game.load_bk_music(this.sound[0]);
        }

        if (this.carrot.get_status_sound())
            this.sound[0].Play();
        else
            this.sound[0].Stop();

        if (this.panel_play.activeInHierarchy)
        {
            if (this.is_play_p_vs_p)
                this.control_p_vs_p.gameObject.SetActive(true);
            else
                this.control_p_vs_ai.gameObject.SetActive(true);
        }
    }

    public void btn_back_home()
    {
        this.carrot.ads.show_ads_Interstitial();
        this.carrot.play_sound_click();
        this.panel_menu.SetActive(true);
        this.panel_play.SetActive(false);
        this.control_p_vs_ai.gameObject.SetActive(false);
        this.control_p_vs_p.gameObject.SetActive(false);
        this.obj_effect_sakura.SetActive(true);
    }

    public void btn_show_user()
    {
        this.carrot.user.show_login();
    }

    public void btn_show_rank()
    {
        this.carrot.game.Show_List_Top_player();
    }

    public void btn_show_rate()
    {
        this.carrot.show_rate();
    }

    public void btn_show_share()
    {
        this.carrot.show_share();
    }

    public void show_result_game()
    {
        if (this.is_play_p_vs_p) 
            this.add_game_total_score(this.control_p_vs_p.black_scores());
        else
            this.add_game_total_score(this.control_p_vs_ai.black_scores());

        this.play_sound(1);
    }

    private void add_game_total_score(int add_scores)
    {
        this.game_total_score += add_scores;
        PlayerPrefs.SetInt("game_total_score", this.game_total_score);
        this.carrot.game.update_scores_player(this.game_total_score);
        this.txt_total_score.text = this.game_total_score.ToString();
    }

    public void play_sound(int index_sound)
    {
        if (this.carrot.get_status_sound()) this.sound[index_sound].Play();
    }

    public void btn_tip()
    {
        this.carrot.play_sound_click();
        if (this.is_play_p_vs_p)
            this.control_p_vs_p.find_pos();
        else
            this.control_p_vs_ai.find_pos();
    }
}
