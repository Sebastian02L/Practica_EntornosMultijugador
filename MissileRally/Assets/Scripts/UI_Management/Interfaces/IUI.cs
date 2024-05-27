using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUI
{
    IState State { get; set; }
    GameObject Canvas { get; set; }
}
