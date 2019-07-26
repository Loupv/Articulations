import oscP5.*;
import netP5.*;
import java.util.Collections;
import java.util.Comparator;

OscP5 oscP5;
OscMessage myMessage;
OscP5 multicastOsc;

ArrayList<SummarySample> dataListP1, dataListP2;

MyLock myLock;

float plottedMs = 20000.0;

void setup() {
  size(1200, 1000);
  frameRate(30);


  // The following is needed for Macs to get the Multicast
  System.setProperty("java.net.preferIPv4Stack", "true");

  myLock = new MyLock();
  dataListP1 = new ArrayList<SummarySample>();
  dataListP2 = new ArrayList<SummarySample>();

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
  text("p2speedLeft", 10 +  width/2, (height * 0) + tSize);
  text("p1speedRight", 10, (height * 0.02) + tSize);
  text("p2speedRight", 10 +  width/2, (height * 0.02) + tSize);
  fill(85, 100, 100);
  text("p1accLeft", 10, (height * 0.04) + tSize);
  text("p2accLeft", 10 +  width/2, (height * 0.04) + tSize);
  text("p1accRight", 10, (height * 0.06) + tSize);
  text("p2accRight", 10 +  width/2, (height * 0.06) + tSize);
  fill(95, 100, 100);
  text("p1contractionLeft", 10, (height * 0.1) + tSize);
  text("p2contractionLeft", 10+  width/2, (height * 0.1) + tSize);

  


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
        stroke(50, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f1 * -1000.0) + (height * 0.12), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width/2 +width/2), 
          (sum1.f1 * -1000.0) + (height * 0.12));

        // acc Left
        stroke(85, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f2 * -10000.0) + (height * 0.25), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2 +width/2), 
          (sum1.f2 * -10000.0) + (height * 0.25));

        // speed Right
        stroke(50, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f6 * -1000.0) + (height * 0.37), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width/2 +width/2), 
          (sum1.f6 * -1000.0) + (height * 0.37));

        // acc Right
        stroke(85, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f7 * -10000.0) + (height * 0.49), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2 +width/2), 
          (sum1.f7 * -10000.0) + (height * 0.49));
          
          
          // contraction
          stroke(95, 100, 100);      
           line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2+width/2), 
           (sum0.h1 * -5.0) + (height * 0.62), 
           width - ((xStart - sum1.timeStamp) / plottedMs * width/2+width/2), 
           (sum1.h1 * -5.0) + (height * 0.62));
      }
    }
  }





  if (dataListP2.size() > 1)
  {

    float lastTimeStamp = dataListP2.get(dataListP2.size()-1).timeStamp;

    for (int i = 1; i < dataListP2.size(); i++)
    {
      if (dataListP2.get(i).timeStamp < lastTimeStamp - plottedMs ) dataListP2.remove(i);
    }

    // dernier temps arrondi à la seconde + nombre de secondes à afficher 
    float xStart = lastTimeStamp - (lastTimeStamp % 10000) + 10000;

    for (int i = 1; i < dataListP2.size(); i++)
    {
      SummarySample sum0 = dataListP2.get(i-1);
      SummarySample sum1 = dataListP2.get(i);
      if (sum1.timeStamp - sum0.timeStamp < 2000)
      {   

        // (lastTimeStamp - (lastTimeStamp % 10000) + plottedM - sum0.timestamp) / plottedMs
        // (lastTimeStamp - (lastTimeStamp % 10000) - sum0.timestamp) / plottedMs + 1

        // temps max en sec - temps actuel / plottedMs

        // speed Left
        
        if ((xStart - sum0.timeStamp) / plottedMs <1) {
          stroke(50, 100, 100);     
          line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
            (sum0.f1 * -1000.0) + (height * 0.12), 
            width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
            (sum1.f1 * -1000.0) + (height * 0.12));

          // acc Left
          stroke(85, 100, 100); 
          line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
            (sum0.f2 * -10000.0) + (height * 0.25), 
            width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
            (sum1.f2 * -10000.0) + (height * 0.25));

          // speed Right
          stroke(50, 100, 100);     
          line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
            (sum0.f6 * -1000.0) + (height * 0.37), 
            width - ((xStart - sum1.timeStamp)  / plottedMs * width/2), 
            (sum1.f6 * -1000.0) + (height * 0.37));

          // acc Right
          stroke(85, 100, 100);      
          line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
            (sum0.f7 * -10000.0) + (height * 0.49), 
            width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
            (sum1.f7 * -10000.0) + (height * 0.49));
            
            
          // contraction
          stroke(95, 100, 100);      
           line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
           (sum0.h1 * -5.0) + (height * 0.62), 
           width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
           (sum1.h1 * -5.0) + (height * 0.62));
           
        }
      }
    }
  }
}



/* incoming osc message are forwarded to the oscEvent method. */
void oscEvent(OscMessage message)
{
  if (message.checkAddrPattern("/P1") == true)
  {
    float f1 = message.get(0).floatValue();
    float f2 = message.get(1).floatValue();
    float f3 = message.get(2).floatValue();
    float f4 = message.get(3).floatValue();
    float f5 = message.get(4).floatValue();
    float f6 = message.get(5).floatValue();
    float f7 = message.get(6).floatValue();
    float f8 = message.get(7).floatValue();
    float f9 = message.get(8).floatValue();
    float f10 = message.get(9).floatValue();
    String timeStamp = message.get(10).stringValue();
    float h1 = message.get(11).floatValue();

    dataListP1.add(new SummarySample(f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, timeStamp, h1));
  } else if (message.checkAddrPattern("/P2") == true)
  {

    float f1 = message.get(0).floatValue();
    float f2 = message.get(1).floatValue();
    float f3 = message.get(2).floatValue();
    float f4 = message.get(3).floatValue();
    float f5 = message.get(4).floatValue();
    float f6 = message.get(5).floatValue();
    float f7 = message.get(6).floatValue();
    float f8 = message.get(7).floatValue();
    float f9 = message.get(8).floatValue();
    float f10 = message.get(9).floatValue();
    String timeStamp = message.get(10).stringValue();
    float h1 = message.get(11).floatValue();


    dataListP2.add(new SummarySample(f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, timeStamp, h1));
  }
}
