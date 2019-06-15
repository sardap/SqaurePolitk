using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

class LatainSenteceCreator
{
	static LatainSenteceCreator _instance;

	public static LatainSenteceCreator Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new LatainSenteceCreator();
			}

			return _instance;
		}
	}

	ConcurrentStack<string> _sentence = new ConcurrentStack<string>();
	Semaphore _wakeUp = new Semaphore(1, int.MaxValue);
	Semaphore _waitForSentence = new Semaphore(0, int.MaxValue);
	string _dataPath;

	LatainSenteceCreator()
	{
		Thread thread = new Thread(new ThreadStart(SentenceProducer));
		_dataPath = Application.dataPath;
		thread.Start();
	}

	public string GetSentence()
	{
		_waitForSentence.WaitOne();

		_sentence.TryPop(out string result);

		if (_sentence.Count <= 3)
			_wakeUp.Release();

		return result;
	}

	void SentenceProducer()
	{
		List<string> latinWords = new List<string>();

		using (var reader = new StreamReader(_dataPath + "/Resources/Text/LatinWords.csv"))
		{
			while (!reader.EndOfStream)
			{
				var words = reader.ReadLine().Split(',').ToList();
				words.ForEach(i => i.Replace(" ", ""));
				latinWords.AddRange(words);
			}
		}

		string CreateSentence()
		{
			var result = "";

			for (int i = 0; i < Util.SharpRandom.Next(10); i++)
			{
				result = result + " " + latinWords[Util.SharpRandom.Next(0, latinWords.Count)];
			}

			return result;
		}

		int n;
		while (true)
		{
			_wakeUp.WaitOne();
			n = System.Math.Max(10, Factory.Instance.BusyTalkingTextCount());
			for(int i = 0; i < n - _sentence.Count; i++)
			{
				_sentence.Push(CreateSentence());
			}
			_waitForSentence.Release(_sentence.Count);
		}
	}


	

}
