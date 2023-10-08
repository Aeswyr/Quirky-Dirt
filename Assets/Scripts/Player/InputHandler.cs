using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : Singleton<InputHandler>
{
    public Vector2 mousePos {
        get;
        private set;
    }
    public Vector2 dir {
        get;
        private set;
    }
    public ButtonState move {
        get{return buttons[0];}
    }
    public ButtonState primary {
        get {return buttons[1];}
    }
    public ButtonState secondary {
        get {return buttons[2];}
    }
    public ButtonState dodge {
        get {return buttons[3];}
    }
    public ButtonState interact {
        get {return buttons[4];}
    }
    public ButtonState menu {
        get {return buttons[5];}
    }
    public ButtonState pause {
        get {return buttons[6];}
    }
    public ButtonState skill1 {
        get{return buttons[7];}
    }
    public ButtonState skill2 {
        get{return buttons[8];}
    }
    public ButtonState skill3 {
        get{return buttons[9];}
    }
    public ButtonState any {
        get {return buttons[10];}
    }

    [SerializeField] private int buttonCount = 1;
    [SerializeField] private short bufferFrames = 5;
    [SerializeField] private bool bufferEnabled = false;
    private short IDSRC = 0;
    private ButtonState[] buttons;
    private Queue<Dictionary<short, short>> inputBuffer = new Queue<Dictionary<short, short>>();
    private Dictionary<short, short> currentFrame;

    public void Start() {
        buttons = new ButtonState[buttonCount];
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Init(ref IDSRC, this);
    }

    private void FixedUpdate() {
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Reset();

        if (bufferEnabled) {
            UpdateBuffer();
        }
    }

    public void Move(InputAction.CallbackContext ctx) {
        this.dir = ctx.ReadValue<Vector2>();
        this.buttons[0].Set(ctx);
    }

    public void Primary(InputAction.CallbackContext ctx) {
        this.buttons[1].Set(ctx);
    }

    public void Secondary(InputAction.CallbackContext ctx) {
        this.buttons[2].Set(ctx);
    }

    public void Dodge(InputAction.CallbackContext ctx) {
        this.buttons[3].Set(ctx);
    }

    public void Interact(InputAction.CallbackContext ctx) {
        this.buttons[4].Set(ctx);
    }

    public void Menu(InputAction.CallbackContext ctx) {
        this.buttons[5].Set(ctx);
    }

    public void Pause(InputAction.CallbackContext ctx) {
        this.buttons[6].Set(ctx);
    }

    public void Skill1(InputAction.CallbackContext ctx) {
        this.buttons[7].Set(ctx);
    }

    public void Skill2(InputAction.CallbackContext ctx) {
        this.buttons[8].Set(ctx);
    }

    public void Skill3(InputAction.CallbackContext ctx) {
        this.buttons[9].Set(ctx);
    }

    public void Any(InputAction.CallbackContext ctx) {
        this.buttons[10].Set(ctx);
    }

    public void MousePos(InputAction.CallbackContext ctx) {
        this.mousePos = ctx.ReadValue<Vector2>();
    }

    public void FlushBuffer() {
        inputBuffer.Clear();
    }

    public void UpdateBuffer() {
        if (inputBuffer.Count >= bufferFrames)
            inputBuffer.Dequeue();
        currentFrame = new Dictionary<short, short>();
        inputBuffer.Enqueue(currentFrame);
    }

    public void PrintBuffer() {
        string bufferData = $"InputBuffer: count-{inputBuffer.Count}";
        foreach (var frame in inputBuffer)
            if (frame.Count > 0)
                bufferData += $"\n{frame.Count}";
        Debug.Log(bufferData);
    }

    public struct ButtonState {
        private short id;
        private static short    STATE_PRESSED = 0,
                                STATE_RELEASED = 1;
        private InputHandler handler;
        private bool firstFrame;
        public bool down {
            get;
            private set;
        }
        public bool pressed {
            get {
                if (handler.bufferEnabled && handler.inputBuffer != null) {
                    foreach (var frame in handler.inputBuffer) {
                        if (frame.ContainsKey(id) && frame[id] == STATE_PRESSED) {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return down && firstFrame;
            }
        }

        public bool released {
            get {
                if (handler.bufferEnabled && handler.inputBuffer != null) {
                    foreach (var frame in handler.inputBuffer) {
                        if (frame.ContainsKey(id) && frame[id] == STATE_RELEASED) {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return !down && firstFrame;
            }
        }

        public void Set(InputAction.CallbackContext ctx) {
            down = !ctx.canceled;             
            firstFrame = true;

            if (handler.bufferEnabled && handler.currentFrame != null) {
                handler.currentFrame.TryAdd(id, down ? STATE_PRESSED : STATE_RELEASED);
            }
        }
        
        public void Reset() {
            firstFrame = false;
        }

        public void Init(ref short IDSRC, InputHandler handler) {
            id = IDSRC++;
            this.handler = handler;
        }
    }
}
