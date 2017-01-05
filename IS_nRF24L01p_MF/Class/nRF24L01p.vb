﻿Imports System
Imports Microsoft.SPOT.Hardware
Imports Microsoft.SPOT

Public Class nRF24L01P
    Implements IDisposable

#Region "Events"

    ''' <summary>
    ''' Occurs when data is received.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Event OnDataReceived(sender As Object, e As DataReceivedEventArgs)

    ''' <summary>
    ''' Occurs when data is received with bad payload.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Event OnDataReceiveFailed(sender As Object, e As DataReceiveFailedEventArgs)


    ''' <summary>
    ''' Occurs when data is received.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Event OnTransmitFailed(sender As Object, e As DataTransmitFailedEventArgs)

    ''' <summary>
    ''' Occurs when data is successfully transmitted.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Event OnTransmitSuccess(sender As Object, e As EventArgs)

#End Region

#Region "Properties"

    ''' <summary>
    ''' RF channel frequency. It determines the center of the channel used by the nRF24L01p. 
    ''' Number from 0 to 125 are allowed.
    ''' </summary>
    ''' <returns></returns>
    Public Property ChannelFreq As Byte
        Get
            Return m_ChannelFreq
        End Get
        Set(value As Byte)
            If value > 125 Then
                m_ChannelFreq = 125
            ElseIf value < 0 Then
                m_ChannelFreq = 0
            Else : m_ChannelFreq = value
            End If

            WriteRegister(Common.Registers.RF_CH, New Byte() {m_ChannelFreq})

        End Set
    End Property
    Private m_ChannelFreq As Byte

    ''' <summary>
    ''' RF address width.
    ''' </summary>
    ''' <returns></returns>
    Public Property AddressWidth As Common.AddressWidth
        Get
            Return m_AddressWidth
        End Get
        Set(value As Common.AddressWidth)
            If value > 5 Then
                m_AddressWidth = Common.AddressWidth.ADR_5
            ElseIf value < 3 Then
                m_AddressWidth = Common.AddressWidth.ADR_3
            Else : m_AddressWidth = value
            End If

            Dim configByte As Byte
            configByte = 0

            Select Case m_AddressWidth
                Case Common.AddressWidth.ADR_3
                    SetRegisterBit(configByte, False, Common.BitFlags.AW_LOW)
                    SetRegisterBit(configByte, True, Common.BitFlags.AW_HIGH)
                Case Common.AddressWidth.ADR_4
                    SetRegisterBit(configByte, True, Common.BitFlags.AW_LOW)
                    SetRegisterBit(configByte, False, Common.BitFlags.AW_HIGH)
                Case Common.AddressWidth.ADR_5
                    SetRegisterBit(configByte, True, Common.BitFlags.AW_LOW)
                    SetRegisterBit(configByte, True, Common.BitFlags.AW_HIGH)
            End Select

            WriteRegister(Common.Registers.SETUP_AW, New Byte() {configByte})

        End Set
    End Property
    Private m_AddressWidth As Common.AddressWidth

    ''' <summary>
    ''' RF db Power
    ''' </summary>
    ''' <returns></returns>
    Public Property dbPower As Common.dbPower
        Get
            Return m_dbPower
        End Get
        Set(value As Common.dbPower)

            If value > Common.dbPower.PWR_0db Then
                m_dbPower = Common.dbPower.PWR_0db
            ElseIf value < Common.dbPower.PWR_minus_18db Then
                m_dbPower = Common.dbPower.PWR_minus_18db
            Else : m_dbPower = value
            End If

            Dim configByte As Byte()
            configByte = ReadRegister(Common.Registers.RF_SETUP)

            Select Case m_dbPower
                Case Common.dbPower.PWR_minus_18db  '-18dBm
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_PWR_LOW)
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_PWR_HIGH)
                Case Common.dbPower.PWR_minus_12db   '-12dBm
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_PWR_LOW)
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_PWR_HIGH)
                Case Common.dbPower.PWR_minus_6db   '-6dBm
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_PWR_LOW)
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_PWR_HIGH)
                Case Common.dbPower.PWR_0db   '0dBm
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_PWR_LOW)
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_PWR_HIGH)
            End Select

            WriteRegister(Common.Registers.RF_SETUP, New Byte() {configByte(0)})

        End Set
    End Property
    Private m_dbPower As Common.dbPower

    ''' <summary>
    ''' RF Speed
    ''' </summary>
    ''' <returns></returns>
    Public Property Speed As Common.RFSpeed
        Get
            Return m_Speed
        End Get
        Set(value As Common.RFSpeed)

            If value > Common.RFSpeed.HIGH_2000 Then
                m_Speed = Common.RFSpeed.HIGH_2000
            ElseIf value < Common.RFSpeed.LOW_250 Then
                m_Speed = Common.RFSpeed.LOW_250
            Else : m_Speed = value
            End If

            Dim configByte As Byte()
            configByte = ReadRegister(Common.Registers.RF_SETUP)

            Select Case m_Speed
                Case Common.RFSpeed.LOW_250  '250Kbps
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_DR_LOW)
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_DR_HIGH)

                Case Common.RFSpeed.MID_1000   '1Mbps
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_DR_LOW)
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_DR_HIGH)

                Case Common.RFSpeed.HIGH_2000   '2Mbps
                    SetRegisterBit(configByte(0), False, Common.BitFlags.RF_DR_LOW)
                    SetRegisterBit(configByte(0), True, Common.BitFlags.RF_DR_HIGH)
            End Select

            WriteRegister(Common.Registers.RF_SETUP, New Byte() {configByte(0)})

        End Set
    End Property
    Private m_Speed As Common.RFSpeed

#End Region

    Private NordicChipEnablePin As OutputPort
    Private WithEvents NordicInterruptPin As InterruptPort
    Private NordicSPI As SPI

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="SPI"></param>
    ''' <param name="Speed"></param>
    ''' <param name="ChipSelectPin"></param>
    ''' <param name="ChipEnablePin"></param>
    ''' <param name="InterruptPin"></param>
    ''' <remarks></remarks>
    Public Sub New(SPI As SPI.SPI_module, Speed As UInteger, ChipSelectPin As Cpu.Pin, ChipEnablePin As Cpu.Pin, InterruptPin As Cpu.Pin)

        NordicSPI = New SPI(New SPI.Configuration(ChipSelectPin, False, 0, 0, False, True, Speed, SPI))
        NordicInterruptPin = New InterruptPort(InterruptPin, False, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow)
        NordicChipEnablePin = New OutputPort(ChipEnablePin, False)

        '#######################################################################################
        'Vider le TX FIFO
        Dim StatusBuffer() As Byte = Nothing
        StatusBuffer = FlushTXFIFO()
        WriteRegister(Common.Registers.STATUS, New Byte() {CByte(StatusBuffer(0) Or &H10)})

        'Vider le RX FIFO
        FlushRXFIFO()
        WriteRegister(Common.Registers.STATUS, New Byte() {CByte(StatusBuffer(0) Or &H40)})

    End Sub

    Public Function ReadFIFO(width As Byte) As Byte()

        Dim writeBuffer(width) As Byte
        Dim readBuffer(width) As Byte

        writeBuffer(0) = CByte(Common.Commands.R_RX_PAYLOAD)

        NordicSPI.WriteRead(writeBuffer, readBuffer)

        Return readBuffer

    End Function

    Public Function ReadRegister(register As Common.Registers, Optional size As Byte = 1) As Byte()

        Dim writeBuffer = New Byte(size) {}
        Dim readBuffer = New Byte(size) {}

        writeBuffer(0) = CByte(Common.Commands.R_REGISTER) Or CByte(register)

        NordicSPI.WriteRead(writeBuffer, readBuffer)

        Return readBuffer

    End Function

    Public Sub WriteRegister(register As Common.Registers, data() As Byte)

        Dim writeBuffer = New Byte(data.Length) {}

        writeBuffer(0) = CByte(Common.Commands.W_REGISTER) Or CByte(register)
        Array.Copy(data, 0, writeBuffer, 1, data.Length)

        NordicSPI.Write(writeBuffer)

    End Sub

    Public Sub WritePayload(data() As Byte)

        Dim writeBuffer = New Byte(data.Length) {}

        Try
            writeBuffer(0) = Common.Commands.W_TX_PAYLOAD
            Array.Copy(data, 0, writeBuffer, 1, data.Length)

            NordicSPI.Write(writeBuffer)

        Catch ex As Exception
            Debug.Print(ex.Message)
        End Try

    End Sub

    Public Function FlushTXFIFO() As Byte()

        Dim writeBuffer(0) As Byte
        Dim readBuffer(0) As Byte

        Try
            writeBuffer(0) = Common.Commands.FLUSH_TX

            NordicSPI.WriteRead(writeBuffer, readBuffer)
        Catch ex As Exception
            Debug.Print("FlushTXFIFO : " + ex.Message)
        End Try


        Return readBuffer

    End Function

    Public Sub FlushRXFIFO()

        Dim writeBuffer(0) As Byte

        Try
            writeBuffer(0) = Common.Commands.FLUSH_RX

            NordicSPI.Write(writeBuffer)

        Catch ex As Exception
            Debug.Print("FlushRXFIFO : " + ex.Message)
        End Try

    End Sub

    Public Function ReadRXFIFOWidth() As Byte

        Dim writeBuffer(1) As Byte
        Dim readBuffer(1) As Byte

        Try
            writeBuffer(0) = Common.Commands.R_RX_PL_WID

            NordicSPI.WriteRead(writeBuffer, readBuffer)
        Catch ex As Exception
            Debug.Print("ReadRXFIFOWidth : " + ex.Message)
        End Try

        Return readBuffer(1)

    End Function

    Public Sub SetRegisterBit(ByRef registerByte As Byte, value As Boolean, index As Integer)

        Try
            If value Then
                registerByte = registerByte Or CByte(2 ^ index)
            Else
                registerByte = registerByte And CByte(255 - (2 ^ index))
            End If
        Catch ex As Exception
            Debug.Print("SetRegisterBit : " + ex.Message)
        End Try

    End Sub

    Public Sub SetChipEnable(value As Boolean)

        Try
            If value Then
                NordicChipEnablePin.Write(True)
            Else
                NordicChipEnablePin.Write(False)
            End If
        Catch ex As Exception
            Debug.Print("SetChipEnable : " + ex.Message)
        End Try

    End Sub

    Private Sub NordicInterruptPin_OnInterrupt(data1 As UInteger, data2 As UInteger, time As Date) Handles NordicInterruptPin.OnInterrupt
        'Get the STATUS Register
        Dim writeBuffer(1) As Byte
        Dim StatusBuffer(1) As Byte

        writeBuffer(0) = Common.Commands.NOP
        NordicSPI.WriteRead(writeBuffer, StatusBuffer)

        '########################################################
        'Vérifier les bits d'état du status

        If (StatusBuffer(0) And &H10) = &H10 Then    'MAX_RT : Maximum number of retransmit reached
            FlushTXFIFO()
            RaiseEvent OnTransmitFailed(Me, New DataTransmitFailedEventArgs(StatusBuffer(0)))

        End If

        If (StatusBuffer(0) And &H20) = &H20 Then 'TX_DS : Données envoyées
            RaiseEvent OnTransmitSuccess(Me, EventArgs.Empty)

        End If

        If (StatusBuffer(0) And &H40) = &H40 Then 'RX_DR : Données disponible dans le FIFO

            Dim pipeID As Byte
            Dim PayloadLen As Byte

            While True
                'Obtenir le PIPE_ID
                pipeID = CByte((StatusBuffer(0) And &HE) / 2)
                PayloadLen = ReadRXFIFOWidth()
                Dim Payload(PayloadLen - 1) As Byte

                If PayloadLen > 32 Then
                    FlushRXFIFO()
                    RaiseEvent OnDataReceiveFailed(Me, New DataReceiveFailedEventArgs(pipeID, StatusBuffer(0)))

                Else
                    Array.Copy(ReadFIFO(PayloadLen), 1, Payload, 0, PayloadLen)
                    RaiseEvent OnDataReceived(Me, New DataReceivedEventArgs(pipeID, Payload))

                End If

                Dim FIFOStatusBuf(1) As Byte

                FIFOStatusBuf = ReadRegister(Common.Registers.FIFO_STATUS)

                If (FIFOStatusBuf(1) And &H1) = &H1 Then
                    Exit While
                End If

            End While

        End If

        SetRegisterBit(StatusBuffer(0), True, Common.BitFlags.MAX_RT)
        SetRegisterBit(StatusBuffer(0), True, Common.BitFlags.TX_DS)
        SetRegisterBit(StatusBuffer(0), True, Common.BitFlags.RX_DR)
        WriteRegister(Common.Registers.STATUS, New Byte() {StatusBuffer(0)})

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Pour détecter les appels redondants

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: supprimez l'état managé (objets managés).
            End If

            ' TODO: libérez les ressources non managées (objets non managés) et substituez la méthode Finalize() ci-dessous.
            ' TODO: définissez les champs volumineux à null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: substituez Finalize() uniquement si Dispose(ByVal disposing As Boolean) ci-dessus comporte du code permettant de libérer des ressources non managées.
    'Protected Overrides Sub Finalize()
    '    ' Ne modifiez pas ce code. Ajoutez du code de nettoyage dans Dispose(ByVal disposing As Boolean) ci-dessus.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Ce code a été ajouté par Visual Basic pour permettre l'implémentation correcte du modèle pouvant être supprimé.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Ne modifiez pas ce code. Ajoutez du code de nettoyage dans Dispose(disposing As Boolean) ci-dessus.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

#Region "EventArgs"


    Public Class DataReceivedEventArgs
        Inherits EventArgs

        Public DataPipe As Byte
        Public Payload As Byte() = Nothing

        Public Sub New(DataPipe As Byte, Payload As Byte())
            Me.DataPipe = DataPipe
            Me.Payload = Payload
        End Sub

    End Class

    Public Class DataReceiveFailedEventArgs
        Inherits EventArgs

        Public DataPipe As Byte
        Public Status As Byte

        Public Sub New(DataPipe As Byte, Status As Byte)
            Me.DataPipe = DataPipe
            Me.Status = Status
        End Sub

    End Class

    Public Class DataTransmitFailedEventArgs
        Inherits EventArgs

        Public Status As Byte

        Public Sub New(Status As Byte)
            Me.Status = Status
        End Sub

    End Class

#End Region


End Class
