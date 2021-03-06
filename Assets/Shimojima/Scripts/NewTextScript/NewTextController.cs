﻿using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NewTextController : MonoBehaviour
{
    [Header("-ゲームオブジェクト-")]

    [SerializeField]
    private Text scenarioText;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Text autoButtonText;

    [SerializeField]
    private NewTextData newTD;

    [SerializeField]
    private Sprite[] useSprite;

    [SerializeField]
    private Image image;
    [SerializeField]
    private Image image2;
    [SerializeField]
    private Image image3;

    //シナリオデータを各データごとに格納する為の構造体
    [System.Serializable]
    public struct ScenarioData
    {
        public string originText;
        public string characterName;
        public int page;
        public int imageNumber;
    }

    public ScenarioData[] sData;

    private enum TextState
    {
        next,
        standby,
        print,
        end
    }

    [SerializeField, Tooltip("現在の状態を表すステート")]
    TextState tState = 0;

    [Header("-TextData関連-")]

    [SerializeField]
    private string[] texts; //表示するテキストデータを一文字ずつ格納する場所

    [SerializeField, Tooltip("シナリオデータの行数")]
    private int sDataIndex;

    [SerializeField, Tooltip("現在読み込まれている行番号")]
    private int nowIndex;

    private int charCount;  //textsの要素数をカウントする

    [SerializeField, Tooltip("コマンドテキストが有る行数の保管")]
    private List<int> commandLineCount = new List<int>();


    [Header("-時間-")]

    [SerializeField]
    private float time;

    private float autoTime;

    private float targetTime;

    [SerializeField, Range(0.001f, 0.3f), Tooltip("表示間隔")]
    private float displayTextInterval = 0.05f;

    [SerializeField, Range(1f, 3f), Tooltip("オート機能使用時、次のページを表示するまでの待機時間")]
    private float autoDisplayTextInterval = 1;

    private bool auto = false;  //オート機能のON,OFF切り替え用

    public static bool end;
    

    void Start()
    {
        end = false;
    }

    void Update()
    {
        TextDataLoad();
        time += Time.deltaTime;

        if (tState == TextState.end) { end = true; return; }
        PrintText();
    }

    /// <summary>
    /// 初期化関数
    /// </summary>
    public void ResetText()
    {
        scenarioText.text = "";
        tState = TextState.next;
        texts = new string[0];
        commandLineCount = new List<int>();
        sDataIndex = 0;
        nowIndex = 0;
        time = 0;
        image.color = new Color(1, 1, 1, 0);
        image2.color = new Color(1, 1, 1, 0);
        image3.color = new Color(1, 1, 1, 0);
        image.sprite = null;
        image2.sprite = null;
        image3.sprite = null;
        end = false;
    }

    /// <summary>
    /// テキストデータを表示する
    /// </summary>
    private void PrintText()
    {
        AutoPrint();
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.GetTouch(0).position;
                List<RaycastResult> result = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, result);

                foreach (RaycastResult raycastResult in result)
                {
                    if (raycastResult.gameObject.name == "Auto")
                    {
                        break;
                    }
                    else if (raycastResult.gameObject.name == "BgPanel")
                    {
                        if (!auto)
                        {
                            TextSkip();
                        }
                    }
                }
            }
        }
#endif

#if UNITY_EDITOR
        //if (Input.GetMouseButtonDown(0))
        //{
        //    PointerEventData pointer = new PointerEventData(EventSystem.current);
        //    pointer.position = Input.mousePosition;
        //    List<RaycastResult> result = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(pointer, result);

        //    foreach (RaycastResult raycastResult in result)
        //    {
        //        if (raycastResult.gameObject.name == "ScenarioText")
        //        {
        //            if (!auto)
        //            {
        //                TextSkip();
        //            }
        //        }
        //    }
        //}
#endif
        if (nowIndex < sDataIndex)
        {
            SetText();

            DisplayText();
        }
    }

    /// <summary>
    /// textsの中身を表示
    /// </summary>
    private void DisplayText()
    {
        if (tState == TextState.print && time >= targetTime && charCount != texts.Length)
        {
            if (charCount == 0)
            {
                if (sData[nowIndex].characterName != "")
                {
                    nameText.text = sData[nowIndex].characterName;
                }

                if (sData[nowIndex].imageNumber != 0 && sData[nowIndex].imageNumber != 99 && sData[nowIndex].imageNumber != 4 && sData[nowIndex].imageNumber != 1)
                {
                    image.sprite = useSprite[sData[nowIndex].imageNumber - 1];
                    image.color = new Color(1,1,1,1);
                    image2.color = new Color(1, 1, 1, 0);
                    image2.sprite = null;
                    image3.color = new Color(1, 1, 1, 0);
                    image3.sprite = null;
                }
                else if (sData[nowIndex].imageNumber == 1)
                {
                    image.color = new Color(1, 1, 1, 0);
                    image.sprite = null;
                    image2.color = new Color(1, 1, 1, 0);
                    image2.sprite = null;
                    image3.sprite = useSprite[sData[nowIndex].imageNumber - 1];
                    image3.color = new Color(1, 1, 1, 1);
                }
                else if (sData[nowIndex].imageNumber == 4)
                {
                    image.color = new Color(1, 1, 1, 0);
                    image.sprite = null;
                    image3.color = new Color(1, 1, 1, 0);
                    image3.sprite = null;
                    image2.sprite = useSprite[sData[nowIndex].imageNumber - 1];
                    image2.color = new Color(1, 1, 1, 1);
                }
                else if (sData[nowIndex].imageNumber == 99)
                {
                    image.color = new Color(1, 1, 1, 0);
                    image.sprite = null;
                    image2.color = new Color(1, 1, 1, 0);
                    image2.sprite = null;
                    image3.color = new Color(1, 1, 1, 0);
                    image3.sprite = null;
                }
                //表示スペースの調整
                scenarioText.text += " ";
            }
            //Debug.Log(1);
            scenarioText.text += texts[charCount];
            charCount++;
            targetTime = time + displayTextInterval;
        }
        else if (charCount == texts.Length)
        {
            //改行してステートをnextに変更
            scenarioText.text += "\n";
            nowIndex++;
            charCount = 0;
            tState = TextState.next;
        }
    }

    /// <summary>
    /// コマンドテキストの直前行までのテキストを一括で表示する
    /// <para>また、ステートがstandbyの場合ステートをnextに変更する</para>
    /// </summary>
    private void TextSkip()
    {
        if (tState == TextState.standby)
        {
            nowIndex++;
            if (nowIndex == sDataIndex)
            {
                tState = TextState.end;
            }
            else
            {
                scenarioText.text = "";
                tState = TextState.next;
            }
        }
        else
        {
            charCount = 0;
            tState = TextState.standby;
            scenarioText.text = "";

            if (sData[nowIndex].page == 0)
            {
                for (int i = 0; i < commandLineCount[0]; i++)
                {
                    scenarioText.text += " " + sData[i].originText + "\n";
                }
                nowIndex = commandLineCount[0];
            }
            else
            {
                int i = commandLineCount[sData[nowIndex].page - 1];
                int _i = commandLineCount[sData[nowIndex].page];
                nowIndex = i;
                for (int j = i + 1; j < _i; j++)
                {
                    scenarioText.text += " " + sData[j].originText + "\n";
                    nowIndex++;
                }
                nowIndex++;
            }
        }
    }

    /// <summary>
    /// textsに表示するテキストデータを格納する
    /// </summary>
    private void SetText()
    {
        if (tState == TextState.next)
        {
            //配列の初期化
            texts = new string[sData[nowIndex].originText.Length - 1];

            if (sData[nowIndex].originText.Substring(0, 1) != "{")
            {
                //テキストの格納
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i] = sData[nowIndex].originText.Substring(i, 1);
                }
                tState = TextState.print;
                targetTime = time + displayTextInterval;
            }
            else
            {
                //コマンドテキストの処理
                string command = sData[nowIndex].originText.Substring(sData[nowIndex].originText.IndexOf('{') + 1,sData[nowIndex].originText.IndexOf("}") - 1);
                switch (command)
                {
                    case "next":
                        tState = TextState.standby;
                        if (auto) { autoTime = time; }
                        break;
                    case "end":
                        tState = TextState.end;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// TextDataのロードが終わり次第シナリオデータの格納
    /// </summary>
    private void TextDataLoad()
    {
        if (newTD.loadFinish)
        {
            int count = 0;
            //最大行数をセット
            sDataIndex = newTD.textData.Count;

            //最大行数に合わせて配列を初期化
            sData = new ScenarioData[sDataIndex];

            for (int i = 0; i < sDataIndex; i++)
            {
                if (newTD.textData[i].Substring(0, 1) == "{")
                {
                    commandLineCount.Add(i);
                    count++;
                }

                sData[i].page = count;

                //名前が記述されている場合はキャラクターネームをセットする
                if (newTD.textData[i].Substring(0, 1) == "~")
                {
                    string[] s = newTD.textData[i].Split('~');
                    sData[i].characterName = s[1];
                    sData[i].originText = s[2];
                    CImageSerach(sData[i].originText, i);

                }
                else
                {
                    sData[i].characterName = "";
                    sData[i].originText = newTD.textData[i];
                    CImageSerach(sData[i].originText, i);
                }
            }
        }
        newTD.loadFinish = false;
    }

    /// <summary>
    /// イメージがある場合はテキスト表示時に出すようにsDataに格納する
    /// </summary>
    private void CImageSerach(string beforeText, int i)
    {
        if (Regex.IsMatch(beforeText, "cImage.."))
        {
            int x = beforeText.IndexOf("cImage") + 6;
            string s = beforeText.Substring(x, 2);
            sData[i].imageNumber = int.Parse(s);
            string afterText = beforeText.Remove(x - 7, 9);
            sData[i].originText = afterText;
        }
    }

    /// <summary>
    /// 自動でシナリオを進める
    /// </summary>
    private void AutoPrint()
    {
        if (auto)
        {
            if (tState == TextState.standby && time >= autoTime + autoDisplayTextInterval)
            {
                scenarioText.text = "";
                nowIndex++;
                tState = TextState.next;
            }
        }
    }

    /// <summary>
    /// オート機能の切り替え
    /// <para>シーン内のボタンで切り替えます</para>
    /// </summary>
    public void AutoSwitch()
    {
        if (!auto)
        {
            auto = true;
            Animator animator = autoButtonText.GetComponent<Animator>();
            animator.SetBool("On", true);
        }
        else if (auto)
        {
            auto = false;
            Animator animator = autoButtonText.GetComponent<Animator>();
            animator.SetBool("On", false);
        }
    }

    /// <summary>
    /// シナリオをスキップし終了する
    /// </summary>
    public void ScenarioSkip()
    {
        tState = TextState.end;
        end = true;
    }
}
