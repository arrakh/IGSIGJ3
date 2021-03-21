using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class InputManager : MonoBehaviourPunCallbacks
{
    public PlayerController currentPlayer;
    public Slider volumeLevelSlider;
    public AudioSource audioSource;

    private string currentMicDevice = null;
    public bool useMicrophone = false;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;
    public float micThreshold = 0.25f;
    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private float[] clipSampleData;

    private void Start()
    {
        if (useMicrophone) MicrophoneInit();
    }

    private void Update()
    {
        if (currentPlayer == null) return;

        currentPlayer.Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        RecordingUpdate();

        if(Input.GetButtonDown("Push") && !useMicrophone)
        {
            currentPlayer.RPC_Push();
        }
    }

    private void MicrophoneInit()
    {
        audioSource.clip = Microphone.Start(currentMicDevice, true, 10, AudioSettings.outputSampleRate);

        audioSource.Play();

        clipSampleData = new float[sampleDataLength];
    }

    private void RecordingUpdate()
    {
        if (!Microphone.IsRecording(currentMicDevice) && !useMicrophone) return;

        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;
            audioSource.clip.GetData(clipSampleData, audioSource.timeSamples);
            clipLoudness = 0f;
            foreach (var sample in clipSampleData)
            {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength;
        }

        volumeLevelSlider.value = clipLoudness;

        if (clipLoudness >= micThreshold)
        {
            currentPlayer.RPC_Push();
        }
    }
}
