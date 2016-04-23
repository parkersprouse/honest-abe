﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
	public GameObject terrain;
	public List<GameObject> enemies;
	public List<GameObject> props;
	public List<GameObject> decals;
	public GameObject boss;

	public float startSpawnPosition;

	public int screensInLevel = 8;
	public int currentScreen;
	public int spawnYPos = 0;
	public int spawnZPos = 10;
	public int propDensity = 3;
	public int decalDensity = 10;
	public int itemDensity = 1;

	private GameObject _camera;
	private List<Vector3> _occupiedPos;
	private System.Random _rnd;
	private string _levelName;
	private int _levelIndex;
	private int _easyWaveChance;
	private int _mediumWaveChance;
	private int _remainingEnemyDensity;
	private int _enemiesInScreen;
	private float _lastXPos;
	private float _spawnOnClearLocation;
	private bool _canSpawn = true;
	private bool _spawningWaveOnClear;

	// Use this for initialization
	void Start()
	{
		_lastXPos = startSpawnPosition;
		_camera = GameObject.Find("Main Camera");
		_rnd = new System.Random();
		currentScreen = 0;
		_spawningWaveOnClear = false;
		_levelName = SceneManager.GetActiveScene().name;
	}

	// Update is called once per frame
	void Update()
	{
		if (GlobalSettings.currentSceneIsNew)
			currentScreen = 0;

		if (_camera.transform.position.x >= _lastXPos - startSpawnPosition && _canSpawn)
		{
			_canSpawn = false;
			_occupiedPos = new List<Vector3>();
			if (!SpawnBoss())
			{
				if (enemies.Count > 0)
					SpawnEnemies();
				_canSpawn = true;
			}
			if (props.Count > 0)
				SpawnProps();
			if (decals.Count > 0)
				SpawnDecals();
			Debug.Log("Completed generation of screen " + currentScreen);
			currentScreen++;
			_lastXPos += startSpawnPosition;
		}

		_enemiesInScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;
		if (_enemiesInScreen <= 0 && currentScreen > 1 && !_spawningWaveOnClear)
		{
			_spawningWaveOnClear = true;
			SpawnEnemies();
			_spawningWaveOnClear = false;
		}
	}

	private void SpawnTerrain()
	{
		Instantiate(terrain, new Vector3(_lastXPos, spawnYPos, spawnZPos), Quaternion.Euler(0, 0, 0));
	}

	private void SpawnProps()
	{
		for (int i = 0; i < propDensity; i++)
		{
			int r = _rnd.Next(props.Count);
			Instantiate(props[r], GetRandomEmptyPos(1f), Quaternion.Euler(0, 0, 0));
		}
	}

	private void SpawnDecals()
	{
		for (int i = 0; i < decalDensity; i++)
		{
			int r = _rnd.Next(decals.Count);
			Instantiate(decals[r], GetRandomEmptyPos(0.5f), Quaternion.Euler(0, 0, 0));
		}
	}

	private void SpawnEnemies()
	{
		if (_levelName.Equals(GlobalSettings.levelOneSceneName) && currentScreen == 0)
			return;

		_remainingEnemyDensity = 0;

		switch (GetWaveDifficulty())
		{
			case 0:
				_remainingEnemyDensity = _rnd.Next(5, 8);
				break;
			case 1:
				_remainingEnemyDensity = _rnd.Next(8, 12);
				break;
			case 2:
				_remainingEnemyDensity = _rnd.Next(12, 16);
				break;
		}
		Debug.Log("Spawning wave of density " + _remainingEnemyDensity);
		while (_remainingEnemyDensity > 0)
		{
			int r = GetRandomEnemyBasedOnCurrentLevelAndDensity();
			Debug.Log("r = " + r);
			if (r == -1)
				break;
			Instantiate(enemies[r], GetRandomEmptyPos(1f), Quaternion.Euler(0, 0, 0));
			Debug.Log("Enemy type " + r + " spawned, remaining density: " + _remainingEnemyDensity);
		}
	}

	private bool SpawnBoss()
	{
		bool spawn = false;
		if (boss && currentScreen == screensInLevel)
		{
			spawn = true;
			Instantiate(boss, GetRandomEmptyPos(1f), Quaternion.Euler(0, 0, 0));
		}
		return spawn;
	}

	private int GetWaveDifficulty()
	{
		int r = _rnd.Next(101);
		if (_levelName == GlobalSettings.levelOneSceneName)
		{
			// Forest wave breakdown: 55-30-15
			_easyWaveChance = GlobalSettings.minRndForEasyWaveInLevel1;
			_mediumWaveChance = GlobalSettings.minRndForMediumWaveInLevel1;
		}
		else if (_levelName == GlobalSettings.levelTwoSceneName)
		{
			// Battlefield wave breakdown: 33-50-15
			_easyWaveChance = GlobalSettings.minRndForEasyWaveInLevel2;
			_mediumWaveChance = GlobalSettings.minRndForMediumWaveInLevel2;
		}
		else if (_levelName == GlobalSettings.levelThreeSceneName)
		{
			// Ballroom wave breakdown: 0-0-100
			_easyWaveChance = GlobalSettings.minRndForEasyWaveInLevel3;
			_mediumWaveChance = GlobalSettings.minRndForMediumWaveInLevel3;
		}

		if (r >= _easyWaveChance)
		{
			Debug.Log("Easy Wave Spawned");
			return 0;
		}
		else if (r >= _mediumWaveChance)
		{
			Debug.Log("Medium Wave Spawned");
			return 1;
		}
		else
		{
			Debug.Log("Hard Wave Spawned");
			return 2;
		}
	}

	private int GetRandomEnemyBasedOnCurrentLevelAndDensity()
	{
		int r = -1;
		if (_remainingEnemyDensity <= 0)
			return r;

		if (_levelName == GlobalSettings.levelOneSceneName)
		{
			// Ensure that spawned enemies do not reduce _remainingEnemyDensity below 0
			if (_remainingEnemyDensity == 1 || currentScreen <= 5)
				r = 0;
			else
				r = _rnd.Next(2);
		}
		else if (_levelName == GlobalSettings.levelTwoSceneName)
		{
			if (_remainingEnemyDensity == 1)
				r = 0;
			else
				r = _rnd.Next(2);
		}
		else if (_levelName == GlobalSettings.levelThreeSceneName)
		{
			if (_remainingEnemyDensity == 1)
				r = 0;
			else
				r = _rnd.Next(2);
		}
		_remainingEnemyDensity -= r + 1;
		return r;
	}

	private Vector3 GetRandomEmptyPos(float z)
	{
		RectTransform area = (RectTransform)terrain.transform;
		double width = area.rect.width;
		double height = area.rect.height;

		float x = 0;
		float y = 0;
		bool occupied = true;
		int attempts = 0;
		while (occupied && attempts < 2)
		{
			occupied = false;

			// Spawn wave along right edge of camera on clear
			if (_spawningWaveOnClear)
				x = GameObject.Find("RightEdge").transform.position.x;
			else
				x = (float)((width * _rnd.NextDouble() * 2) - width + _lastXPos);
			y = (float)((height * _rnd.NextDouble()) * 0.9 - height);

			foreach (Vector3 pos in _occupiedPos)
			{
				if ((Math.Abs((double)(x - pos.x)) < 1.0) && (Math.Abs((double)(y - pos.y)) < 1.0))
				{
					occupied = true;
					break;
				}
			}
			attempts++;
		}

		Vector3 vector = new Vector3(x, y, z);
		_occupiedPos.Add(vector);
		return vector;
	}
}
