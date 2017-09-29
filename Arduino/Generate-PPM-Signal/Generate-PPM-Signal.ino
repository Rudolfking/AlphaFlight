//////////////////////CONFIGURATION///////////////////////////////
#define chanel_number 4  //set the number of chanels
#define default_servo_value 1500  //set the default servo value
#define offset 20  //set offset from default servo value
#define PPM_FrLen 22500  //set the PPM frame length in microseconds (1ms = 1000µs)
#define PPM_PulseLen 300  //set the pulse length
#define onState 1  //set polarity of the pulses: 1 is positive, 0 is negative
#define sigPin 10  //set PPM signal output pin on the arduino
//////////////////////////////////////////////////////////////////

int ppm[chanel_number];
int current[4];
int next[4];
int oldMillis;

int incomingByte = 0;
String incomingString;
bool rcSysIsFutaba = false;
bool debugMode = false;
bool nextIsAvailable = false;

void setup(){  
  for(int i=0; i<chanel_number; i++){
    ppm[i]= default_servo_value;
  }

  for (int i=0;i<3;i++){
    current[i] = default_servo_value;
    next[i] = default_servo_value;
    }
  ppm[2] = 1500;
  current[3] = 10;
  next[3]= 10;

  pinMode(13, OUTPUT);
  pinMode(sigPin, OUTPUT);
  digitalWrite(sigPin, !onState);  //set the PPM signal pin to the default state (off)
  digitalWrite(13, LOW);

  Serial.begin(9600);
  Serial.setTimeout(50);
  
  cli();
  TCCR1A = 0; // set entire TCCR1 register to 0
  TCCR1B = 0;
  
  OCR1A = 100;  // compare match register, change this
  TCCR1B |= (1 << WGM12);  // turn on CTC mode
  TCCR1B |= (1 << CS11);  // 8 prescaler: 0,5 microseconds at 16mhz
  TIMSK1 |= (1 << OCIE1A); // enable timer compare interrupt
  sei();
}

void loop(){
  current[3] -= millis()-oldMillis;
  oldMillis = millis();
  if (current[3] < 1 || current [3] > 30000){
    if (nextIsAvailable == true){
      for (int i=0; i<4; i++){
          current[i] = next[i];
      }
      
      ppm[0] = current[0] + offset;
      ppm[1] = current[1] + offset;
      ppm[3] = current[2] + offset;
      
      for (int i=0; i<3; i++){
        next[i] = default_servo_value + offset;
      }
      next[3] = 10;
      nextIsAvailable = false;
    } else {
        for (int i = 0;i<4;i++){
          ppm[i] = default_servo_value + offset;
          }
      }
  }
  if (current[3] < 0){
    current[3] = 10;
    }
    
    if (debugMode){
      Serial.println("");
      Serial.print(ppm[0]);
      Serial.print("  |  ");
      Serial.print(ppm[1]);
      Serial.print("  |  ");
      Serial.print(ppm[2]);
      Serial.print("  |  ");
      Serial.print(ppm[3]);
      Serial.print("  |  ");
      Serial.print(current[3]);
    }
}

void serialEvent(){
  if (Serial.available() > 0){
    incomingString = Serial.readString();
  }
  if (getValue(incomingString, ',', 4)=="z" && incomingString.endsWith("z")){
      digitalWrite(13, HIGH);
      Serial.println("");
      Serial.println("");
      Serial.print("OK");
      Serial.println("");
      processData();
    } else if (incomingString == "d") {
        if (debugMode == true){
          debugMode = false;
          Serial.println("");
          Serial.println("Debug mode turned off");
          Serial.println("");
          } else {
              debugMode = true;
              Serial.println("");
              Serial.println("Debug mode turned on");
              Serial.println("");
            }
      } else {
          digitalWrite(13, LOW);
          Serial.println("");
          Serial.println("");
          Serial.print("UNAUTHERIZED");
          Serial.println("");
        }
}

void processData(){
  int temp;
  for (int i=0; i<4; i++){
      next[i] = getValue(incomingString, ',', i).toInt();
  }
  nextIsAvailable = true;
  Serial.println("");
  Serial.print("Outputting ");
  Serial.print(next[0]);
  Serial.print(" roll ");
  Serial.print(next[1]);
  Serial.print(" pitch and ");
  Serial.print(next[2]);
  Serial.print(" yaw for ");
  Serial.print(next[3]);
  Serial.print(" milliseconds in ");
  Serial.print(current[3]);
  Serial.print(" milliseconds");
  Serial.println("");
  }


String getValue(String data, char separator, int index) {
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length()-1;

  for(int i=0; i<=maxIndex && found<=index; i++){
    if(data.charAt(i)==separator || i==maxIndex){
        found++;
        strIndex[0] = strIndex[1]+1;
        strIndex[1] = (i == maxIndex) ? i+1 : i;
    }
  }

  return found>index ? data.substring(strIndex[0], strIndex[1]) : "";
}

ISR(TIMER1_COMPA_vect){  //leave this alone
  static boolean state = true;
  
  TCNT1 = 0;
  
  if(state) {  //start pulse
    digitalWrite(sigPin, onState);
    OCR1A = PPM_PulseLen * 2;
    state = false;
  }
  else{  //end pulse and calculate when to start the next pulse
    static byte cur_chan_numb;
    static unsigned int calc_rest;
  
    digitalWrite(sigPin, !onState);
    state = true;

    if(cur_chan_numb >= chanel_number){
      cur_chan_numb = 0;
      calc_rest = calc_rest + PPM_PulseLen;// 
      OCR1A = (PPM_FrLen - calc_rest) * 2;
      calc_rest = 0;
    }
    else{
      OCR1A = (ppm[cur_chan_numb] - PPM_PulseLen) * 2;
      calc_rest = calc_rest + ppm[cur_chan_numb];
      cur_chan_numb++;
    }     
  }
}
