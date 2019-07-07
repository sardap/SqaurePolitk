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
	Semaphore _sentenceWakeUp = new Semaphore(1, int.MaxValue);
	ConcurrentStack<string> _createdNames = new ConcurrentStack<string>();
	Semaphore _nameWakeUp = new Semaphore(1, int.MaxValue);
	string _dataPath;
	IEnumerable<string> _allNames;

	LatainSenteceCreator()
	{
		_dataPath = Application.dataPath;

		_sentenceWakeUp.Release();
		_nameWakeUp.Release();

		Thread thread = new Thread(new ThreadStart(SentenceProducer));
		thread.Start();

		thread = new Thread(new ThreadStart(NameProducer));
		thread.Start();

		_allNames = LoadStringsFromCSV("/Resources/Text/Names.csv");

	}

	public string GetSentence()
	{
		if(!_sentence.TryPop(out string result))
		{
			_sentenceWakeUp.Release();
			return "";
		}

		if (_sentence.Count <= 3)
			_sentenceWakeUp.Release();

		return result;
	}

	public string GetName()
	{
		if(!_createdNames.TryPop(out string result))
		{
			_nameWakeUp.Release();
			return CreateName(_allNames);
		}

		if (_createdNames.Count <= 3)
			_nameWakeUp.Release();

		return result;
	}

	IEnumerable<string> LoadStringsFromCSV(string path)
	{
		List<string> latinWords = new List<string>();

		using (var reader = new StreamReader(_dataPath + path))
		{
			while (!reader.EndOfStream)
			{
				var words = reader.ReadLine().Split(',').ToList();
				words.ForEach(i => i.Replace(" ", ""));
				latinWords.AddRange(words);
			}
		}

		return latinWords;
	}

	void SentenceProducer()
	{
		var latinWords = LoadStringsFromCSV("/Resources/Text/LatinWords.csv");

		string CreateSentence()
		{
			var result = "";

			for (int i = 0; i < Util.SharpRandom.Next(10); i++)
			{
				result = result + " " + Util.RandomElement(latinWords);
			}

			return result;
		}


		int n;
		while (true)
		{
			_sentenceWakeUp.WaitOne();
			n = System.Math.Max(10, Factory.Instance.BusyTalkingTextCount());
			for(int i = 0; i < n - _sentence.Count; i++)
			{
				_sentence.Push(CreateSentence());
			}
		}
	}

	string CreateName(IEnumerable<string> allNames)
	{
		var fullName = new HashSet<string>();

		while (fullName.Count < 3)
		{
			fullName.Add(Util.RandomElement(allNames));
		}

		return fullName.Aggregate((a, b) => a + " " + b);
	}


	void NameProducer()
	{
		var names = LoadStringsFromCSV("/Resources/Text/Names.csv");

		int n;
		while (true)
		{
			_nameWakeUp.WaitOne();
			n = 10;
			for (int i = 0; i < n - _sentence.Count; i++)
			{
				_sentence.Push(CreateName(names));
			}
		}
	}




}
