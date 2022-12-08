#include <msp430x16x.h>

int adc1,adc2,adc3,adc4,adc5,adc6;
unsigned char Packet[7];

int lpfilt1(int indata);

float a[4]={1.0000 , -2.7488 , 2.5282 , -0.7776}; // 5Hz LPF 데이터 변수
float b[4]={0.0002196 , 0.0006588 , 0.0006588 , 0.0002196}; // 5Hz LPF 데이터변수
void ReadAdc12 (void); // Read data from internal 12 bits ADC

void main(void)
{
  unsigned int i;
  
// Set basic clock and timer
    WDTCTL = WDTPW + WDTHOLD; // Stop WDT
    BCSCTL1 &= ~XT2OFF; // // XT2 on
    do{
    IFG1 &=~OFIFG; // Clear oscillator flag
    for(i=0;i<0xFF;i++); // Delay for OSC to stabilize
    }while((IFG1&OFIFG)); 
    
    BCSCTL2 |= SELM_2; // MCLK =XT2CLK=6Mhz
    BCSCTL2 |= SELS; // SMCLK=XT2CLK=6Mhz

// Set Port 
    P3SEL = BIT4|BIT5; // P3.4,5 = USART0 TXD/RXD
    P6SEL = 0x3f; P6DIR=0x3f; P6OUT=0x00; 

//Set UART0 
    ME1 |= UTXE0 + URXE0; // Enable USART0 TXD/RXD
    UCTL0 |= CHAR; // 8-bit character
    UTCTL0 |= SSEL0|SSEL1; // UCLK= SMCLK
    UBR00 = 0x34; // 6MHz 115200
    UBR10 = 0x00; // 6MHz 115200
    UMCTL0 = 0x00; // 6MHz 115200 modulation
    UCTL0 &= ~SWRST; // Initialize USART state machine
    
// Set 12bit internal ADC
    ADC12CTL0 = ADC12ON | REFON | REF2_5V; // ADC on, 2.5 V reference on
    ADC12CTL0 |= MSC; // multiple sample and conversion
    ADC12CTL1 = ADC12SSEL_3 | ADC12DIV_7 | CONSEQ_1; // SMCLK, /8, sequence of channels
    
    ADC12CTL1 |= SHP; 
    ADC12MCTL0 = SREF_0 | INCH_0;
    ADC12MCTL1 = SREF_0 | INCH_1;
    ADC12MCTL2 = SREF_0 | INCH_2;
    ADC12MCTL3 = SREF_0 | INCH_3;
    ADC12MCTL4 = SREF_0 | INCH_4;
    ADC12MCTL5 = SREF_0 | INCH_5 | EOS;
    ADC12CTL0 |= ENC; // enable conversion
    
// SetTimerA
    TACTL=TASSEL_2+MC_1; // clock source and mode(UP) select
    TACCTL0=CCIE;
    TACCR0=24000; // 6M/24000=250hz
 
 _BIS_SR(LPM0_bits + GIE); // Enter LPM0 w/ interrupt 
}

#pragma vector = TIMERA0_VECTOR
__interrupt void TimerA0_interrupt()
{
 int filtdata1 = 7000;
   
  ReadAdc12();
 
 Packet[0]=(unsigned char)0x81; // 헤더가 81
__no_operation();

 filtdata1=lpfilt1(adc4)+5000-650;
 //filtdata2=lpfilt2(adc4)+5000-650;
 
    Packet[1]=(unsigned char)(filtdata1>>7)&0x7F;
    Packet[2]=(unsigned char)filtdata1&0x7F;
 
    Packet[3]=0;
    Packet[4]=0;
 
    Packet[5]=0;
    Packet[6]=0; 
 
    for(int j=0;j<7;j++){
        while (!(IFG1 & UTXIFG0)); // USART0 TX buffer ready?
        TXBUF0=Packet[j];
    }
 }
 
void ReadAdc12 (void)
{
    // read ADC12 result from ADC12 conversion memory
    // start conversion and store result without CPU intervention
    adc1 = (int)( (long)ADC12MEM0 * 9000 / 4096) -4500+7000; // adc0 voltage in [mV]
    adc2 = (int)( (long)ADC12MEM1 * 9000 / 4096) -4500+7000; 
    adc3 = (int)( (long)ADC12MEM2 * 9000 / 4096) -4500+7000; 
    adc4 = (int)( (long)ADC12MEM3 * 9000 / 4096) -4500+7000; 
    adc5 = (int)( (long)ADC12MEM4 * 9000 / 4096) -4500+7000; 
    adc6 = (int)( (long)ADC12MEM5 * 9000 / 4096) -4500+7000; 
 
    ADC12CTL0|=ADC12SC; // start conversion
}

int lpfilt1(int indata) //adc1
{ 
 static float y0=0,y1=0,y2=0,y3=0;
 static int x0=0,x1=0,x2=0,x3=0;
 int output;
 x0=indata;
 //filtering
 y0=b[0]*x0+b[1]*x1+b[2]*x2+b[3]*x3-a[1]*y1-a[2]*y2-a[3]*y3;
 y3=y2;
 y2=y1;
 y1=y0;
 x3=x2;
 x2=x1;
 x1=x0;
 output=(int)y0;
 return(output);
}