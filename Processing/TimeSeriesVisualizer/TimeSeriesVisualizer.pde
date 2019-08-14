import oscP5.*;
import netP5.*;
import java.util.Collections;
import java.util.Comparator;

OscP5 oscP5;
OscMessage myMessage;
OscP5 multicastOsc;

ArrayList<SummarySample> dataListP1, dataListP2;

float plottedMs = 20000.0;

void setup() {
  size(1200, 1000);
  frameRate(30);


  // The following is needed for Macs to get the Multicast
  System.setProperty("java.net.preferIPv4Stack", "true");

  dataListP1 = new ArrayList<SummarySample>();
  
  oscP5 = new OscP5(this, 7979);
  //initializeReceiving();
}

void draw() {
  background(0);

  strokeWeight(2);

  colorMode(HSB, 100);

  int tSize = 22;

  // header text
  textSize(tSize);
  fill(50, 100, 100);
  text("p1speedLeft", 10, (height * 0) + tSize);
  text("p1speedRight", 10, (height * 0.02) + tSize);
  fill(85, 100, 100);
  text("p1accLeft", 10, (height * 0.04) + tSize);
  text("p1accRight", 10, (height * 0.06) + tSize);
  fill(95, 100, 100);
  text("p1contractionLeft", 10, (height * 0.1) + tSize);

  


  if (dataListP1.size() > 1)
  {
    //stroke((15*listInd) % 100, 100, 100);
    float lastTimeStamp = dataListP1.get(dataListP1.size()-1).timeStamp;

    for (int i = 1; i < dataListP1.size()-1; i++)
    {
      if (dataListP1.get(i).timeStamp < lastTimeStamp - plottedMs ) dataListP1.remove(i);
    }

    float xStart = lastTimeStamp - (lastTimeStamp % 10000)  + 10000;

    for (int i = 1; i < dataListP1.size(); i++)
    {

      SummarySample sum0 = dataListP1.get(i-1);
      SummarySample sum1 = dataListP1.get(i);

      if (sum1.timeStamp - sum0.timeStamp < 2000)
      {    
        //println(sum0.timeStamp+", "+sum0.f1);

        // speed Left
        /*stroke(50, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
          (sum0.f1 * -4000.0) + (height * 0.80), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width), 
          (sum1.f1 * -4000.0) + (height * 0.80));

        // acc Left
        stroke(85, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
          (sum0.f2 * -4000.0) + (height * 0.60), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width), 
          (sum1.f2 * -4000.0) + (height * 0.60));
          
        stroke(15, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
          (sum0.f3 * -4000.0) + (height * 0.40), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width), 
          (sum1.f3 * -4000.0) + (height * 0.40));*/
          
          
        stroke(50, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
          (sum0.f1 * -10000.0) + (height * 0.80), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width), 
          (sum1.f1 * -10000.0) + (height * 0.80));  
          
        stroke(0, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
          (sum0.f4 * -1.0) + (height * 0.40), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width), 
          (sum1.f4 * -1.0) + (height * 0.40));
        println(sum0.f1);
      }
    }
  }

  
}



/* incoming osc message are forwarded to the oscEvent method. */
void oscEvent(OscMessage message)
{
  if (message.checkAddrPattern("/Tests") == true)
  {

    float f1 = message.get(0).floatValue();
    float f2 = message.get(1).floatValue();
    float f3 = message.get(2).floatValue();
    float f4 = message.get(4).intValue();
    
    String timeStamp = message.get(3).stringValue();
    //float h1 = message.get(11).floatValue();


    dataListP1.add(new SummarySample(f1, f2, f3, f4, timeStamp));
  }
}
