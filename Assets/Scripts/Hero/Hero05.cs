using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;

//【长者】当你的星星 消亡后，你不会因此死亡并获得<获胜：弹药+5>，三个回合后，你死亡
public class Hero05 : Hero
{
	//不灭愤怒会造成星星变成燃烧的星星，燃烧星星可以一直被打
	//不灭愤怒持续时间在每回合开始--（防止计算当前回合）
	//判断游戏结束时额外判断是否time==0
	//不灭愤怒时弹药+5
	public bool isDying = false;
	public int dyingTime = 3;
	public int extraAmmos = 5;

	private BoardScript starBoard;
	private float hVal = 0f;
	private PlayerManager player = null;

	private void FixedUpdate()
	{
		if(isDying)
		{
			hVal+=180*Time.deltaTime;
			if(hVal>=360)
			{
				hVal -= 360;
			}
			starBoard.gameObject.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(hVal/360f, 15/100f, 1f);
		}
	}
	#region override
	public override bool OnMyStarRuined()
	{
		return true;
	}
	public override bool OnRoundStart()
	{
		if(isDying)
		{
			dyingTime--;
		}
		return false;
	}
	public override int GetExtraAmmos()
	{
		return isDying ? extraAmmos : 0;
	}
	public override void Ability(BoardScript board)
	{
		//不灭的愤怒！
		if(!isDying)
		{
			isDying = true;
			starBoard = board;//Update开启无敌星星动画
			board.ChangeSprite(BoardScript.BoardState.Star);
		}
	}
	public override bool OnAttackOver()
	{
		if(player==null)
		{
			player = GameManager.Instance.GetPlayerByActorNumber(playerNum);
		}
		//持续结束
		if(isDying && dyingTime<=0)
		{
			isDying = false;//脱离濒死
			starBoard.gameObject.GetComponent<SpriteRenderer>().color = Color.white;//回归正常
			starBoard.ChangeSprite(BoardScript.BoardState.DamagedStar);//不堪重负
			player.myHp = 0;//安详去世
		}
		return true;
	}
	#endregion
}
