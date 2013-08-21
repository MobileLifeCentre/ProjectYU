/*
 * Copyright 2012 Dynastream Innovations Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
package com.dsi.ant.sample.acquirechannels;

import android.os.RemoteException;
import android.util.Log;

import com.dsi.ant.channel.AntChannel;
import com.dsi.ant.channel.AntCommandFailedException;
import com.dsi.ant.channel.IAntChannelEventHandler;
import com.dsi.ant.message.ChannelId;
import com.dsi.ant.message.ChannelType;
import com.dsi.ant.message.fromant.AcknowledgedDataMessage;
import com.dsi.ant.message.fromant.BroadcastDataMessage;
import com.dsi.ant.message.fromant.ChannelEventMessage;
import com.dsi.ant.message.fromant.MessageFromAntType;
import com.dsi.ant.message.ipc.AntMessageParcel;

import java.util.Random;

public class ChannelController
{
    private static final int CHANNEL_PROOF_DEVICE_TYPE = 0x08;
    private static final int CHANNEL_PROOF_TRANSMISSION_TYPE = 1;
    
    private static final int CHANNEL_PROOF_PERIOD = 32768; // 1 Hz
    private static final int CHANNEL_PROOF_FREQUENCY = 77;
    
    private static final String TAG = ChannelController.class.getSimpleName();
    
    private static Random randGen = new Random();
    
    private AntChannel mAntChannel;
    private ChannelBroadcastListener mChannelBroadcastListener;
    
    private ChannelEventCallback mChannelEventCallback = new ChannelEventCallback();

    private ChannelInfo mChannelInfo;
    
    private boolean mIsOpen;

    static public abstract class ChannelBroadcastListener
    {
        public abstract void onBroadcastChanged(ChannelInfo newInfo);
    }
    
    public ChannelController(AntChannel antChannel, boolean isMaster, int deviceId, 
            ChannelBroadcastListener broadcastListener)
    {
        mAntChannel = antChannel;
        mChannelInfo = new ChannelInfo(deviceId, isMaster, randGen.nextInt(256));
        mChannelBroadcastListener = broadcastListener;
        
        openChannel();
    }
    
    
    boolean openChannel()
    {
        if(null != mAntChannel)
        {
            if(mIsOpen)
            {
                Log.w(TAG, "Channel was already open");
            }
            else
            {
                /*
                 * Although this reference code sets ChannelType to either a transmitting master or a receiving slave,
                 * the standard for ANT is that channels communication is bidirectional. The use of single-direction 
                 * communication in this app is for ease of understanding as reference code. For more information and
                 * any additional features on ANT channel communication, refer to the ANT Protocol Doc found at: 
                 * http://www.thisisant.com/resources/ant-message-protocol-and-usage/
                 */
                ChannelType channelType = (mChannelInfo.isMaster ? 
                        ChannelType.BIDIRECTIONAL_MASTER : ChannelType.BIDIRECTIONAL_SLAVE);

                ChannelId channelId = new ChannelId(mChannelInfo.deviceNumber, 
                        CHANNEL_PROOF_DEVICE_TYPE, CHANNEL_PROOF_TRANSMISSION_TYPE);
                
                try
                {
                    mAntChannel.setChannelEventHandler(mChannelEventCallback);
                    
                    mAntChannel.assign(channelType);
                    /*
                     * For any additional ANT features such as proximity search or background scanning, refer to 
                     * the ANT Protocol Doc found at: 
                     * http://www.thisisant.com/resources/ant-message-protocol-and-usage/
                     */
                    mAntChannel.setChannelId(channelId);
                    mAntChannel.setPeriod(CHANNEL_PROOF_PERIOD);
                    mAntChannel.setRfFrequency(CHANNEL_PROOF_FREQUENCY);
                    mAntChannel.open();
                    mIsOpen = true;

                    Log.d(TAG, "Opened channel with device number: " + mChannelInfo.deviceNumber);
                } catch (RemoteException e) {
                    channelError(e);
                } catch (AntCommandFailedException e) {
                    // This will release, and therefore unassign if required
                    channelError("Open failed", e);
                }
            }
        }
        else
        {
            Log.w(TAG, "No channel available");
        }
        
        return mIsOpen;
    }

    public class ChannelEventCallback implements IAntChannelEventHandler
    {
        private void updateData(byte[] data) {
            mChannelInfo.broadcastData = data;

            mChannelBroadcastListener.onBroadcastChanged(mChannelInfo);
        }

        @Override
        public void onChannelDeath()
        {
            displayChannelError("Channel Death");
        }
        
        @Override
        public void onReceiveMessage(MessageFromAntType messageType, AntMessageParcel antParcel) {
            Log.d(TAG, "Rx: "+ antParcel);

            switch(messageType)
            {
                case BROADCAST_DATA:
                 // Rx Data
                    updateData(new BroadcastDataMessage(antParcel).getPayload());
                    break;
                case ACKNOWLEDGED_DATA:
                    // Rx Data
                    updateData(new AcknowledgedDataMessage(antParcel).getPayload());
                    break;
                case CHANNEL_EVENT:
                    ChannelEventMessage eventMessage = new ChannelEventMessage(antParcel);
                    
                    switch(eventMessage.getEventCode())
                    {
                        case TX:
                            // Use old info as this is what remote device has just received
                            mChannelBroadcastListener.onBroadcastChanged(mChannelInfo);

                            mChannelInfo.broadcastData[0]++;

                            if(mIsOpen)
                            {
                                try {
                                    mAntChannel.setBroadcastData(mChannelInfo.broadcastData);
                                } catch (RemoteException e) {
                                    channelError(e);
                                }
                            }
                            break;
                        case RX_SEARCH_TIMEOUT:
                         // TODO May want to keep searching
                            displayChannelError("No Device Found");
                            break;
                        case CHANNEL_CLOSED:
                        case CHANNEL_COLLISION:
                        case RX_FAIL:
                        case RX_FAIL_GO_TO_SEARCH:
                        case TRANSFER_RX_FAILED:
                        case TRANSFER_TX_COMPLETED:
                        case TRANSFER_TX_FAILED:
                        case TRANSFER_TX_START:
                        case UNKNOWN:
                         // TODO More complex communication will need to handle these events
                            break;
                    }
                    break;
                case ANT_VERSION:
                case BURST_TRANSFER_DATA:
                case CAPABILITIES:
                case CHANNEL_ID:
                case CHANNEL_RESPONSE:
                case CHANNEL_STATUS:
                case SERIAL_NUMBER:
                case OTHER:
                 // TODO More complex communication will need to handle these message types
                    break;
            }
        }
    }
    
    
    public ChannelInfo getCurrentInfo()
    {
        return mChannelInfo;
    }
    
    void displayChannelError(String displayText)
    {
        mChannelInfo.die(displayText);
        mChannelBroadcastListener.onBroadcastChanged(mChannelInfo);
    }
    
    void channelError(RemoteException e) {
        String logString = "Remote service communication failed.";
                
        Log.e(TAG, logString);
        
        displayChannelError(logString);
    }
    
    void channelError(String error, AntCommandFailedException e) {
        String initiatingMessageId = "0x"+ Integer.toHexString(
                e.getResponseMessage().getInitiatingMessageId());
        String rawResponseCode = "0x"+ Integer.toHexString(
                e.getResponseMessage().getRawResponseCode());
        
        StringBuilder logString = new StringBuilder(error)
                .append(". Command ")
                .append(initiatingMessageId)
                .append(" failed with code ")
                .append(rawResponseCode);
                
        Log.e(TAG, logString.toString());
        
        mAntChannel.release();
        
        displayChannelError("ANT Command Failed");
    }
    
    
    public void close()
    {
        // TODO kill all our resources
        if (null != mAntChannel)
        {
            mIsOpen = false;
            
            mAntChannel.release();
            mAntChannel = null;
        }
        
        displayChannelError("Channel Closed");
    }
}
