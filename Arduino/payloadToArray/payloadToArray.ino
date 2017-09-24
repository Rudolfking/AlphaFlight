void setup() {
  // put your setup code here, to run once:
 Serial.begin(9600);
 Serial.setTimeout(20);
}

String incomingString;
String wordd;
long int toPrint;

void loop() {
   if (Serial.available() > 0){
    incomingString = Serial.readString();
    for (int i = 0; i<3; i++) {
    wordd = getValue(incomingString, ',', i);
    Serial.println(wordd);
    Serial.println(getInt(incomingString, ',', i));
    Serial.println(getInt(incomingString, ',', i)*2);
  }
   }
}
  
String getValue(String data, char separator, int index)
{
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

int getInt(String data, char separator, int index){
  String temp;
  temp = getValue(data, separator, index);
  return temp.toInt();
  }

//int foo = Integer.parseInt("1234");
