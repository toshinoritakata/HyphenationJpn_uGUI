using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Text;
using System;

[RequireComponent(typeof(Text))]
[ExecuteInEditMode]
public class HyphenationJpn : UIBehaviour
{
	// http://answers.unity3d.com/questions/424874/showing-a-textarea-field-for-a-string-variable-in.html
	[TextArea(3,10), SerializeField]
	private string text;

	private float spaceSize;

	public bool updateEditorOnly = true;

	private RectTransform _RectTransform{
		get{
			if( _rectTransform == null )
				_rectTransform = GetComponent<RectTransform>();
			return _rectTransform;
		}
	}
	private RectTransform _rectTransform;

	private Text _Text{
		get{
			if( _text == null )
				_text = GetComponent<Text>();
			return _text;
		}
	}
	private Text _text;

	protected override void OnRectTransformDimensionsChange ()
	{
		base.OnRectTransformDimensionsChange();
		if (updateEditorOnly && Application.isPlaying){ return; } // run only editor

		UpdateText(text);

	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (updateEditorOnly && Application.isPlaying){ return; } // run only editor
		
		UpdateText(text);
	}

	void UpdateText(string str)
	{
		// update Text
		_Text.text = SetText(_Text, str);
	}
	
	public void SetText(string str)
	{
		text = str;
		UpdateText(text);
	}


	string SetText(Text textComp, string msg)
	{
		if(string.IsNullOrEmpty(msg)){
			return string.Empty;
		}
		
		float w = _RectTransform.rect.width;
		
		// get space width
		textComp.text = "m m";
		float tmp0 = textComp.preferredWidth;
		textComp.text = "mm";
		float tmp1 = textComp.preferredWidth;
		spaceSize = (tmp0 - tmp1);
		
		// override
		textComp.horizontalOverflow = HorizontalWrapMode.Overflow;


		// work
		StringBuilder line = new StringBuilder();

		List<string> wordList = GetWordList(msg);
		
		float lineW = 0;
		for(int i = 0; i < wordList.Count; i++){

			textComp.text = wordList[i];
			lineW += textComp.preferredWidth;

			if(wordList[i] == Environment.NewLine){
				lineW = 0;
			}else{
				if(wordList[i] == " "){
					lineW += spaceSize;
				}
				if(lineW > w){
					line.Append( Environment.NewLine );
					textComp.text = wordList[i];
					lineW = textComp.preferredWidth;
				}
			}
			line.Append( wordList[i] );
		}
		
		return line.ToString();
	}

	private List<string> GetWordList(string tmpText)
	{
		List<string> words = new List<string>();
		
		StringBuilder line = new StringBuilder();
		for(int j = 0; j < tmpText.Length; j ++){

			char currentCharacter = tmpText[j];//single Charactor
			char nextCharacter = new char();
			if(j < tmpText.Length-1){
				nextCharacter = tmpText[j+1];
			}
			char preCharacter = new char();
			if(j > 0){
				preCharacter = tmpText[j-1];
			}

			if( IsLatin(currentCharacter) && !IsLatin(preCharacter) ){
				words.Add(line.ToString());
				line = new StringBuilder();
				continue;
			}

			line.Append( currentCharacter );
			
			if( (!IsLatin(currentCharacter) && CHECK_HYP_BACK(preCharacter)) ||
			    (!IsLatin(nextCharacter) && !CHECK_HYP_FRONT(nextCharacter) && !CHECK_HYP_BACK(currentCharacter))||
			    (j == tmpText.Length - 1)){
				words.Add(line.ToString());
				line = new StringBuilder();
				continue;
			}
		}
		return words;
	}

	// helper
	public float textWidth{
		set{
			_RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
		}
		get{
			return _RectTransform.rect.width;
		}
	}
	public int fontSize
	{
		set{
			_Text.fontSize = value;
		}
		get{
			return _Text.fontSize;
		}
	}

	// static

	// 禁則処理 http://ja.wikipedia.org/wiki/%E7%A6%81%E5%89%87%E5%87%A6%E7%90%86
	// 行頭禁則文字
	private static char[] HYP_FRONT = 
		(",)]｝、。）〕〉》」』】〙〗〟’”｠»" +// 終わり括弧類 簡易版
		 "ァィゥェォッャュョヮヵヶっぁぃぅぇぉっゃゅょゎ" +//行頭禁則和字 
		 "‐゠–〜ー" +//ハイフン類
		 "?!‼⁇⁈⁉" +//区切り約物
		 "・:;" +//中点類
		 "。.").ToCharArray();//句点類
	private static char[] HYP_BACK = "([｛〔〈《「『【〘〖〝‘“｟«".ToCharArray();//始め括弧類
	private static char[] HYP_LATIN = 
		("abcdefghijklmnopqrstuvwxyz" +
	     "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + 
	     "0123456789" + 
	     "<>().,]").ToCharArray();

	private static bool CHECK_HYP_FRONT(char str)
	{
		return Array.Exists<char>(HYP_FRONT, item => item == str);
	}

	private static bool CHECK_HYP_BACK(char str)
	{
		return Array.Exists<char>(HYP_BACK, item => item == str);
	}

	private static bool IsLatin(char s)
	{
		return Array.Exists<char>(HYP_LATIN, item => item == s);
	}
}