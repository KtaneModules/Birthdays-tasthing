using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class birthdays : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMNeedyModule module;

    public KMSelectable screen;
    public KMSelectable left;
    public KMSelectable right;
    private TextMesh screenText;
    private TextMesh leftText;
    private TextMesh rightText;

    private bool active;
    private int index;
    private static readonly string[] ids = new string[] { "4314", "5986", "2463", "6903", "7431", "1167", "1518", "6835", "5301", "5448", "4414", "4192", "6320", "5765", "0607", "3002", "7314", "0551" };
    private static readonly string[] days = new string[] { "11", "26", "15", "20", "17", "17", "03", "09", "20", "24", "07", "19", "18", "05", "22", "12", "25", "21" };
    private static readonly string[] months = new string[] { "09", "08", "12", "10", "09", "12", "07", "11", "12", "01", "05", "02", "02", "03", "01", "06", "03", "10" };

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;
    private bool bombSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        module.OnNeedyActivation += OnNeedyActivation;
        module.OnNeedyDeactivation += OnNeedyDeactivation;
        module.OnTimerExpired += OnTimerExpired;
        bomb.OnBombExploded += delegate () { bombSolved = true; };
        bomb.OnBombSolved += delegate () { bombSolved = true; };
        screen.OnInteract += delegate () { PressScreen(); return false; };
        left.OnInteract += delegate () { PressLeft(); return false; };
        right.OnInteract += delegate () { PressRight(); return false; };
    }

    void Start()
    {
        Debug.LogFormat("[Birthdays #{0}] Needy initiated.", moduleId);
        module.SetResetDelayTime(45f, 60f);
        screenText = screen.GetComponentInChildren<TextMesh>();
        leftText = left.GetComponentInChildren<TextMesh>();
        rightText = right.GetComponentInChildren<TextMesh>();
        screenText.text = "";
        leftText.text = "";
        rightText.text = "";
    }

    protected void OnNeedyActivation()
    {
        active = true;
        leftText.text = rnd.Range(1, 32).ToString("00");
        rightText.text = rnd.Range(1, 13).ToString("00");
        index = rnd.Range(0, ids.Length);
        screenText.text = ids[index];
        Debug.LogFormat("[Birthdays #{0}] The display says {1}. Submit {2} {3}.", moduleId, ids[index], days[index], months[index]);
    }

    protected void OnNeedyDeactivation()
    {
        active = false;
        screenText.text = "";
        leftText.text = "";
        rightText.text = "";
    }

    void PressScreen()
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, screen.transform);
        screen.AddInteractionPunch(.1f);
        if (!active)
            return;
        if (leftText.text == days[index] && rightText.text == months[index])
        {
            module.OnPass();
            OnNeedyDeactivation();
        }
        else
            module.OnStrike();
    }

    void PressLeft()
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
        screen.AddInteractionPunch(.1f);
        if (!active)
            return;
        leftText.text = ((Array.IndexOf(Enumerable.Range(0, 31).Select(x => x.ToString("00")).ToArray(), leftText.text)) + 1).ToString("00");
        if (leftText.text == "00")
            leftText.text = "01";
    }

    void PressRight()
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
        screen.AddInteractionPunch(.1f);
        if (!active)
            return;
        rightText.text = ((Array.IndexOf(Enumerable.Range(0, 31).Select(x => x.ToString("00")).ToArray(), rightText.text)) + 1).ToString("00");
        if (rightText.text == "13")
            rightText.text = "01";
    }

    protected void OnTimerExpired()
    {
        if (active)
        {
            module.OnStrike();
            OnNeedyDeactivation();
        }
    }

    // Twitch Plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} 31 12 [Submits December 31st. Single digit numbers must be submitted with leading zeroes, e.g. March 5th is 05 03.]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var input = cmd.Trim().ToLowerInvariant().Split(' ').ToArray();
        if (input.Length != 2 || !Enumerable.Range(1, 31).Select(x => x.ToString("00")).ToArray().Contains(input[0]) || !Enumerable.Range(1, 12).Select(x => x.ToString("00")).ToArray().Contains(input[1]))
            yield break;
        yield return null;
        while (leftText.text != input[0])
        {
            left.OnInteract();
            yield return new WaitForSeconds(.1f);
        }
        while (rightText.text != input[1])
        {
            right.OnInteract();
            yield return new WaitForSeconds(.1f);
        }
        yield return new WaitForSeconds(.1f);
        screen.OnInteract();
    }

    void TwitchHandleForcedSolve()
    {
        //The code is done in a coroutine instead of here so that if the solvebomb command was executed this will just input the number right when it activates and it wont wait for its turn in the queue
        StartCoroutine(HandleSolve());
    }

    IEnumerator HandleSolve()
    {
        while (!bombSolved)
        {
            while (!active)
                yield return new WaitForSeconds(.1f);
            while (leftText.text != days[index])
            {
                left.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            while (rightText.text != months[index])
            {
                right.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            yield return new WaitForSeconds(.1f);
            screen.OnInteract();
        }
    }
}
