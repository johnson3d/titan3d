using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.DesignMacross.Base.Description
{

    public interface IDataPinOperator
    {
        public List<TtDataInPinDescription> DataInPins { get; }
        public List<TtDataOutPinDescription> DataOutPins { get; }
        public void AddDataInPin(TtDataInPinDescription pinDescription);

        public bool RemoveDataInPin(TtDataInPinDescription pinDescription);

        public void AddDataOutPin(TtDataOutPinDescription pinDescription);

        public bool RemoveDataOutPin(TtDataOutPinDescription pinDescription);
        public bool IsDataPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin);
        public bool TryGetLinkableDataInPins(TtDataPinDescription targetPin, out List<TtDataInPinDescription> outLinkablePins)
        {
            var pins = new List<TtDataInPinDescription>();
            foreach (var pin in DataInPins)
            {
                if (IsDataPinsLinkable(pin, targetPin))
                {
                    pins.Add(pin);
                }
            }
            outLinkablePins = pins;
            if (pins.Count > 0)
            {
                return true;
            }
            return false;
        }
        public bool TryGetLinkableDataOutPins(TtDataPinDescription targetPin, out List<TtDataOutPinDescription> outLinkablePins)
        {
            var pins = new List<TtDataOutPinDescription>();
            foreach (var pin in DataOutPins)
            {
                if (IsDataPinsLinkable(pin, targetPin))
                {
                    pins.Add(pin);
                }
            }
            outLinkablePins = pins;
            if (pins.Count > 0)
            {
                return true;
            }
            return false;
        }
        public void OnDataPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription);
        public void OnDataPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, IDescription graphDescription);

        public static void AddDataInPin(IDescription operatorDescription, TtDataInPinDescription pinDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.AddDataInPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }
        public static void RemoveDataInPin(IDescription operatorDescription, TtDataInPinDescription pinDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.RemoveDataInPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }
        public static void AddDataOutPin(IDescription operatorDescription, TtDataOutPinDescription pinDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.AddDataOutPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }
        public static void RemoveDataOutPin(IDescription operatorDescription, TtDataOutPinDescription pinDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.RemoveDataOutPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }
        public static bool IsDataPinsLinkable(IDescription operatorDescription, TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            if(operatorDescription is IDataPinOperator dataPinOperator)
            {
                return dataPinOperator.IsDataPinsLinkable(selfPin, targetPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
                return false;
            }
        }
        public static void OnDataPinConnected(IDescription operatorDescription, TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.OnDataPinConnected(selfPin, connectedPin, graphDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }
        public static void OnDataPinDisConnected(IDescription operatorDescription, TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, IDescription graphDescription)
        {
            if (operatorDescription is IDataPinOperator dataPinOperator)
            {
                dataPinOperator.OnDataPinDisConnected(selfPin, disConnectedPin, graphDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataPinOperator");
            }
        }

    }
    public interface IExecutionPinOperator
    {
        public List<TtExecutionInPinDescription> ExecutionInPins { get; }
        public List<TtExecutionOutPinDescription> ExecutionOutPins { get; }
        public void AddExecutionInPin(TtExecutionInPinDescription pinDescription);
        public bool RemoveExecutionInPin(TtExecutionInPinDescription pinDescription);
        public void AddExecutionOutPin(TtExecutionOutPinDescription pinDescription);
        public bool RemoveExecutionOutPin(TtExecutionOutPinDescription pinDescription);

        public static void AddExecutionInPin(IDescription operatorDescription, TtExecutionInPinDescription pinDescription)
        {
            if (operatorDescription is IExecutionPinOperator executionPinOperator)
            {
                executionPinOperator.AddExecutionInPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionPinOperator");
            }
        }
        public static void RemoveExecutionInPin(IDescription operatorDescription, TtExecutionInPinDescription pinDescription)
        {
            if (operatorDescription is IExecutionPinOperator executionPinOperator)
            {
                executionPinOperator.RemoveExecutionInPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionPinOperator");
            }
        }
        public static void AddExecutionOutPin(IDescription operatorDescription, TtExecutionOutPinDescription pinDescription)
        {
            if (operatorDescription is IExecutionPinOperator executionPinOperator)
            {
                executionPinOperator.AddExecutionOutPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionPinOperator");
            }
        }
        public static void RemoveExecutionOutPin(IDescription operatorDescription, TtExecutionOutPinDescription pinDescription)
        {
            if (operatorDescription is IExecutionPinOperator executionPinOperator)
            {
                executionPinOperator.RemoveExecutionOutPin(pinDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionPinOperator");
            }
        }
    }
    public interface IDataLineOperator
    {
        public List<TtDataLineDescription> DataLines { get; }
        public void AddDataLine(TtDataLineDescription lineDescription);
        public bool RemoveDataLine(TtDataLineDescription lineDescription);
        public TtDataPinDescription GetLinkedDataPin(TtDataPinDescription dataPin);
        public TtDataLineDescription GetDataLineWithPin(TtDataPinDescription dataPin);
        public List<TtDataLineDescription> GetDataLinesWithPin(TtDataPinDescription dataPin);
        public bool ContainsDataLineBetweenPins(TtDataPinDescription pinA, TtDataPinDescription pinB);
        public bool ContainsDataLineBetweenPins(Guid pinA, Guid pinB);

        public static void AddDataLine(IDescription operatorDescription, TtDataLineDescription lineDescription)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                dataLineOperator.AddDataLine(lineDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
            }
        }

        public static void RemoveDataLine(IDescription operatorDescription, TtDataLineDescription lineDescription)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                dataLineOperator.RemoveDataLine(lineDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
            }
        }

        public static TtDataPinDescription GetLinkedDataPin(IDescription operatorDescription, TtDataPinDescription dataPin)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                return dataLineOperator.GetLinkedDataPin(dataPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
                return null;
            }
        }

        public static TtDataLineDescription GetDataLineWithPin(IDescription operatorDescription, TtDataPinDescription dataPin)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                return dataLineOperator.GetDataLineWithPin(dataPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
                return null;
            }
        }
        public static List<TtDataLineDescription> GetDataLinesWithPin(IDescription operatorDescription, TtDataPinDescription dataPin)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                return dataLineOperator.GetDataLinesWithPin(dataPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
                return null;
            }
        }

        public static bool ContainsDataLineBetweenPins(IDescription operatorDescription, TtDataPinDescription pinA, TtDataPinDescription pinB)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                return dataLineOperator.ContainsDataLineBetweenPins(pinA, pinB);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
                return false;
            }
        }
        public static bool ContainsDataLineBetweenPins(IDescription operatorDescription, Guid pinA, Guid pinB)
        {
            if (operatorDescription is IDataLineOperator dataLineOperator)
            {
                return dataLineOperator.ContainsDataLineBetweenPins(pinA, pinB);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IDataLineOperator");
                return false;
            }
        }

        public static (TtDataLineDescription fromPinLine, TtDataLineDescription toPinLine) GetExistSingleLinkDataLine(IDescription graphDesc, TtDataPinDescription fromPin, TtDataPinDescription toPin = null)
        {
            (TtDataLineDescription fromPinLine, TtDataLineDescription toPinLine) existLinePair;
            existLinePair.fromPinLine = null;
            existLinePair.toPinLine = null;

            if (fromPin != null && !fromPin.CanMutilLink)
            {
                existLinePair.fromPinLine = IDataLineOperator.GetDataLineWithPin(graphDesc, fromPin);
            }
            if (toPin != null && !toPin.CanMutilLink)
            {
                existLinePair.toPinLine = IDataLineOperator.GetDataLineWithPin(graphDesc, toPin);
            }
            return existLinePair;
        }
        public static void AddSingleDataLine(IDescription graphDesc, TtDataLineDescription lineToBeAdded, (TtDataLineDescription fromPinLine, TtDataLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                IDataLineOperator.RemoveDataLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                IDataLineOperator.RemoveDataLine(graphDesc, existLines.toPinLine);
            }
            IDataLineOperator.AddDataLine(graphDesc, lineToBeAdded);
        }
        public static void RemoveSingleDataLine(IDescription graphDesc, TtDataLineDescription lineToBeRemoved, (TtDataLineDescription fromPinLine, TtDataLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                IDataLineOperator.AddDataLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                IDataLineOperator.AddDataLine(graphDesc, existLines.toPinLine);
            }
            IDataLineOperator.RemoveDataLine(graphDesc, lineToBeRemoved);
        }
        public static bool TryGetAvailableDataLineWithPin(IDataPinOperator pinOperator, TtDataPinDescription startPin, out TtDataLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtDataInPinDescription)
            {
                if (pinOperator.TryGetLinkableDataOutPins(startPin, out var linkablePins))
                {
                    if (linkablePins.Count > 0)
                    {
                        var firstPin = linkablePins[0];
                        fromId = firstPin.Id;
                        fromDescName = firstPin.Parent.Name;
                        toId = startPin.Id;
                        toDescName = startPin.Parent.Name;
                        existLink = true;
                    }
                }
            }
            else
            {
                if (pinOperator.TryGetLinkableDataInPins(startPin, out var linkablePins))
                {
                    if (linkablePins.Count > 0)
                    {
                        var firstPin = linkablePins[0];
                        fromId = startPin.Id;
                        fromDescName = startPin.Parent.Name;
                        toId = firstPin.Id;
                        toDescName = firstPin.Parent.Name;
                        existLink = true;
                    }
                }
            }
            if (existLink)
            {
                line = new TtDataLineDescription() { Name = "Data_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
    }
    public interface IExecutionLineOperator
    {
        public List<TtExecutionLineDescription> ExecutionLines { get; }
        public void AddExecutionLine(TtExecutionLineDescription lineDescription);
        public bool RemoveExecutionLine(TtExecutionLineDescription lineDescription);
        public bool ContainsExecutionLineBetweenPins(TtExecutionPinDescription pinA, TtExecutionPinDescription pinB);
        public bool ContainsExecutionLineBetweenPins(Guid pinA, Guid pinB);
        public TtExecutionPinDescription GetLinkedExecutionPin(TtExecutionPinDescription execPin);
        public TtExecutionLineDescription GetExecutionLineWithPin(TtExecutionPinDescription execPin);

        public static void AddExecutionLine(IDescription operatorDescription, TtExecutionLineDescription lineDescription)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                executionLineOperator.AddExecutionLine(lineDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
            }
        }
        public static void RemoveExecutionLine(IDescription operatorDescription, TtExecutionLineDescription lineDescription)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                executionLineOperator.RemoveExecutionLine(lineDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
            }
        }

        public static TtExecutionPinDescription GetLinkedExecutionPin(IDescription operatorDescription, TtExecutionPinDescription execPin)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                return executionLineOperator.GetLinkedExecutionPin(execPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
                return null;
            }
        }
        public static TtExecutionLineDescription GetExecutionLineWithPin(IDescription operatorDescription, TtExecutionPinDescription execPin)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                return executionLineOperator.GetExecutionLineWithPin(execPin);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
                return null;
            }
        }
        public static bool ContainsExecutionLineBetweenPins(IDescription operatorDescription, TtExecutionPinDescription pinA, TtExecutionPinDescription pinB)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                return executionLineOperator.ContainsExecutionLineBetweenPins(pinA, pinB);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
                return false;
            }
        }
        public static bool ContainsExecutionLineBetweenPins(IDescription operatorDescription, Guid pinA, Guid pinB)
        {
            if (operatorDescription is IExecutionLineOperator executionLineOperator)
            {
                return executionLineOperator.ContainsExecutionLineBetweenPins(pinA, pinB);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExecutionLineOperator");
                return false;
            }
        }

        public static (TtExecutionLineDescription fromPinLine, TtExecutionLineDescription toPinLine) GetExistSingleLinkExecutionLine(IDescription graphDesc, TtExecutionPinDescription fromPin, TtExecutionPinDescription toPin = null)
        {
            (TtExecutionLineDescription fromPinLine, TtExecutionLineDescription toPinLine) existLinePair;
            existLinePair.fromPinLine = null;
            existLinePair.toPinLine = null;

            if (fromPin != null && !fromPin.CanMutilLink)
            {
                existLinePair.fromPinLine = GetExecutionLineWithPin(graphDesc, fromPin);
            }
            if (toPin != null && !toPin.CanMutilLink)
            {
                existLinePair.toPinLine = GetExecutionLineWithPin(graphDesc, toPin);
            }
            return existLinePair;
        }
        public static void AddSingleExecutionLine(IDescription graphDesc, TtExecutionLineDescription lineToBeAdded, (TtExecutionLineDescription fromPinLine, TtExecutionLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                RemoveExecutionLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                RemoveExecutionLine(graphDesc, existLines.toPinLine);
            }
            AddExecutionLine(graphDesc, lineToBeAdded);
        }
        public static void RemoveSingleExecutionLine(IDescription graphDesc, TtExecutionLineDescription lineToBeRemoved, (TtExecutionLineDescription fromPinLine, TtExecutionLineDescription toPinLine) existLines)
        {
            if (existLines.fromPinLine != null)
            {
                AddExecutionLine(graphDesc, existLines.fromPinLine);
            }
            if (existLines.toPinLine != null)
            {
                AddExecutionLine(graphDesc, existLines.toPinLine);
            }
            RemoveExecutionLine(graphDesc, lineToBeRemoved);
        }

        public static bool TryGetLinkedExecutionLineWithPin(IExecutionPinOperator pinOperator, TtExecutionPinDescription startPin, out TtExecutionLineDescription line)
        {
            var fromId = Guid.Empty;
            var fromDescName = "";
            var toId = Guid.Empty;
            var toDescName = "";
            bool existLink = false;
            if (startPin is TtExecutionInPinDescription)
            {
                var pins = pinOperator.ExecutionOutPins;
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = firstPin.Id;
                    fromDescName = firstPin.Parent.Name;
                    toId = startPin.Id;
                    toDescName = startPin.Parent.Name;
                    existLink = true;
                }
            }
            else
            {
                var pins = pinOperator.ExecutionInPins;
                if (pins.Count > 0)
                {
                    var firstPin = pins[0];
                    fromId = startPin.Id;
                    fromDescName = startPin.Parent.Name;
                    toId = firstPin.Id;
                    toDescName = firstPin.Parent.Name;
                    existLink = true;
                }
            }
            if (existLink)
            {
                line = new TtExecutionLineDescription() { Name = "Exec_" + fromDescName + "_To_" + toDescName, FromId = fromId, ToId = toId };
                return true;
            }
            else
            {
                line = null;
                return false;
            }
        }
    }

    public interface IStatementOperator
    {
        public List<TtStatementDescription> Statements { get; }
        public void AddStatement(TtStatementDescription statementDescription);
        public bool RemoveStatement(TtStatementDescription statementDescription);

        public static void AddStatement(IDescription operatorDescription, TtStatementDescription statementDescription)
        {
            if (operatorDescription is IStatementOperator statementOperator)
            {
                statementOperator.AddStatement(statementDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IStatementOperator");
            }
        }
        public static void RemoveStatement(IDescription operatorDescription, TtStatementDescription statementDescription)
        {
            if (operatorDescription is IStatementOperator statementOperator)
            {
                statementOperator.RemoveStatement(statementDescription);
            }
                else
                {
                    System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IStatementOperator");
            }
        }
    }

    public interface IExpressionOperator
    {
        public List<TtExpressionDescription> Expressions { get; }
        public void AddExpression(TtExpressionDescription expressionDescription);
        public bool RemoveExpression(TtExpressionDescription expressionDescription);

        public static void AddExpression(IDescription operatorDescription, TtExpressionDescription expressionDescription)
        {
            if (operatorDescription is IExpressionOperator expressionOperator)
            {
                expressionOperator.AddExpression(expressionDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExpressionOperator");
            }
        }
        public static void RemoveExpression(IDescription operatorDescription, TtExpressionDescription expressionDescription)
        {
            if (operatorDescription is IExpressionOperator expressionOperator)
            {
                expressionOperator.RemoveExpression(expressionDescription);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Operator {operatorDescription.Name} does not implement IExpressionOperator");
            }
        }
    }
}
