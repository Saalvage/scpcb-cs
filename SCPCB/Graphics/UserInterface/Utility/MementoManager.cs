namespace SCPCB.Graphics.UserInterface.Utility;

using Return = (string Text, int Caret, int CaretWanderer);

public readonly record struct Memento(string Content, int StartPosition, bool WasWrite, int Caret, int CaretWanderer, bool Linked) {
    public string Execute(string str, bool redo) {
        if (WasWrite ^ redo) {
            return str[..StartPosition] + str[(StartPosition + Content.Length)..];
        } else {
            return str[..StartPosition] + Content + str[StartPosition..];
        }
    }
}

public class MementoManager {
    private const int MAX_COUNT = 100;

    private readonly LinkedList<Memento> _history = [];

    private readonly LinkedListNode<Memento> _beginSentinel = new(default);
    private readonly LinkedListNode<Memento> _endSentinel = new(default);
    private LinkedListNode<Memento> _index;

    public MementoManager() {
        _history.AddFirst(_beginSentinel);
        _history.AddLast(_endSentinel);
        _index = _endSentinel;
    }

    public void Submit(int caretAfter, Memento memento) {
        while (_index.Previous != _beginSentinel) {
            _history.Remove(_index.Previous!);
        }

        _index = _history.AddAfter(_beginSentinel, memento);
        _beginSentinel.Value = new("", 0, false, caretAfter, caretAfter, false);

        while (_history.Count > MAX_COUNT) {
            _endSentinel.Value = _endSentinel.Previous!.Previous!.Value;
            _history.Remove(_endSentinel.Previous!);
        }
    }

    public Return Undo(string str, bool wasLinked = false) {
        if (_index == _endSentinel) {
            return (str, _endSentinel.Value.Caret, _endSentinel.Value.CaretWanderer);
        }

        var ret = (_index.Value.Execute(str, false), _index.Value.Caret, _index.Value.CaretWanderer);
        var shouldContinue = !wasLinked && _index.Value.Linked;
        _index = _index.Next!;
        return shouldContinue ? Undo(ret.Item1, true) : ret;
    }

    public Return Redo(string str, bool wasLinked = false) {
        if (_index == _beginSentinel || _index.Previous == _beginSentinel) {
            return (str, _beginSentinel.Value.Caret, _beginSentinel.Value.CaretWanderer);
        }

        _index = _index.Previous!;
        var preValue = _index.Previous!.Value;

        var ret = (_index.Value.Execute(str, true), preValue.Caret, preValue.CaretWanderer);
        var shouldContinue = !wasLinked && _index.Value.Linked;
        return shouldContinue ? Redo(ret.Item1, true) : ret;
    }
}
