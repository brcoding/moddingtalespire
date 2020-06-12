import random
from time import sleep
import threading
# This is the SDK, we are importing functions we wish to use.
from moddingtalessdk import *

wee_block = 'H4sIAAAAAAAACzv369xFRgZGBo5L542SD5u47BDvXjp/3VtxRgYQOLD///96OxAN5NgzMDTYQWjccgBKaq1WUAAAAA=='

def spiralPrint(m, n, a, height) : 
    k = 0; l = 0
  
    ''' k - starting row index 
        m - ending row index 
        l - starting column index 
        n - ending column index 
        i - iterator '''
      
  
    while (k < m and l < n) : 
          
        # Print the first row from 
        # the remaining rows  
        for i in range(l, n) : 
            CreateSlab(k, height, i, wee_block)
            sleep(0.05)
            #print(a[k][i], end = " ") 
              
        k += 1
  
        # Print the last column from 
        # the remaining columns  
        for i in range(k, m) : 
            CreateSlab(i, height, n - 1, wee_block)
            sleep(0.05)
            # print(a[i][n - 1], end = " ") 
              
        n -= 1
  
        # Print the last row from 
        # the remaining rows  
        if ( k < m) : 
              
            for i in range(n - 1, (l - 1), -1) : 
                CreateSlab(m - 1, height, i, wee_block)
                sleep(0.05)
                # print(a[m - 1][i], end = " ") 
              
            m -= 1
          
        # Print the first column from 
        # the remaining columns  
        if (l < n) : 
            for i in range(m - 1, k - 1, -1) : 
                CreateSlab(i, height, l, wee_block)
                sleep(0.05)
                # print(a[i][l], end = " ") 
              
            l += 1
def addRat(x, y, z):
    AddCreature("d8c8a8e0-c452-48ad-9ecd-aba5e54cf235", x, y, z, 0, "Test Rat", 10, 10, 10, 10,
        10, 10, 10, 10, 10, 10, False, False)

def spiralCreature(m, n, a, height) : 
    k = 0; l = 0
  
    ''' k - starting row index 
        m - ending row index 
        l - starting column index 
        n - ending column index 
        i - iterator '''
      
  
    while (k < m and l < n) : 
          
        # Print the first row from 
        # the remaining rows  
        for i in range(l, n) : 
            addRat(k, height, i)
            sleep(0.05)
            #print(a[k][i], end = " ") 
              
        k += 1
  
        # Print the last column from 
        # the remaining columns  
        for i in range(k, m) : 
            addRat(i, height, n - 1)
            sleep(0.05)
            # print(a[i][n - 1], end = " ") 
              
        n -= 1
  
        # Print the last row from 
        # the remaining rows  
        if ( k < m) : 
              
            for i in range(n - 1, (l - 1), -1) : 
                addRat(m - 1, height, i)
                sleep(0.05)
                # print(a[m - 1][i], end = " ") 
              
            m -= 1
          
        # Print the first column from 
        # the remaining columns  
        if (l < n) : 
            for i in range(m - 1, k - 1, -1) : 
                addRat(i, height, l)
                sleep(0.05)
                # print(a[i][l], end = " ") 
              
            l += 1
  

def main():
    # print(GetCreatureAssets())
    # # create giant rat
    # print(AddCreature("d8c8a8e0-c452-48ad-9ecd-aba5e54cf235", 0, 0.5, 0, 0, "Test Rat", 10, 10, 10, 10,
    #      10, 10, 10, 10, 10, 10, False, False))
    # print(AddCreature("d8c8a8e0-c452-48ad-9ecd-aba5e54cf235", 2, 0.5, 0, 0, "Test Rat", 10, 10, 10, 10,
    #      10, 10, 10, 10, 10, 10, False, False))
    # print(AddCreature("d8c8a8e0-c452-48ad-9ecd-aba5e54cf235", 4, 0.5, 0, 0, "Test Rat", 10, 10, 10, 10,
    #      10, 10, 10, 10, 10, 10, False, False))
    #MoveCamera(0, 0, 0, True)

    a = [ [1, 2, 3, 4, 5, 6], 
          [7, 8, 9, 10, 11, 12], 
          [13, 14, 15, 16, 17, 18] ] 
            
    R = 3; C = 6
    #for height in range(0, 20):
    spiralCreature(R, C, a, 0) 

    # # Driver Code 
    # a = [ [1, 2, 3, 4, 5, 6], 
    #       [7, 8, 9, 10, 11, 12], 
    #       [13, 14, 15, 16, 17, 18] ] 
            
    # R = 3; C = 6
    # for height in range(0, 20):
    #     spiralPrint(R, C, a, height * 0.5) 
    
    #print(CreateSlab(0, 0, 0, wee_block))
    # h = 4
    # v = 4
    # while h > 0 and v > 0:
    #     for x in range(h):
    #         for y in range(v):
    #             #CreateSlab(h, 0, v, wee_block)
    #         v -= 1
    #     CreateSlab(h, 0, v, wee_block)
    #     h -= 1

    # slabs = [
    #     '```H4sIAAAAAAAAC32SsQkCQRREx9jEwFTYyDLUw8gyrMTBCuzAzDrmGjiwAwMzOzASOVkM9t4muzDwHn//DO/hPtNMx8t1/rytu/N19dovT4etvic7yZvfvR3fHqOaGTI1MgPTwDQwBUwBU8CsMwiywL9AlkZm8Bl8Bp/BJ/AJfAKfyCfYkWBHgr0HmAFmoJ/ANDANzAAzwAww1cPsPczeTzMLMAswCzDr/gRZoBOQNXsW8AV8AV+mfQafwWfwGXwBX8AX8AV8tS+t/+ynfbWDkLV8BXwFfAV8BXz/fVl04/ux+wAjduDUGAcAAA==```',
    #     '```H4sIAAAAAAAAC32WsU0DQRBF1zGJA1KkiYipwIxFREYJSwdUACsiQjqAiDb424AlOnBARgdEWKfdE8HMs3TSyU//3c3NaLSH38PXpmzK/evb2ffH5f757eLn5vzp9rEsPy+l7U7X9bjG/X/mnrN3YEdg233OHnrO7oBdAdsCOwrqA+bA2mfOXoJ3qT3vw2RRHxxyDjmDnEGuQK5ATspzk0W5BrkGuYUn37ooZw2YgM3aI2bAHFgFNusL56zAfApyynMNcg1ygpwgt85Z9J49zxnkDHIOOYdchVyF3OxRuF8GC/fSYOE+EzgFToFzPC/crYOFO3mwcJcLnAKncuecwaj2yaLaJ4tqFzgFToFzzm7Yow496rnTwGngNHA6OB2cDs4KzgrOCs7Zv2iWJotmabJolgROgVPgnP0LZ77DzPfcaeA0cBo4HZwOTgdnBWcFZwXn0sNdco4cLDx/DhaeWwVOgVPgHM+jM8ryf+IMc8pz6z4Lalj3YOBc92fABE6BU+Bc91n0rXvuXPdnwAycBk4Dp4PTwengrOCs4KzgXM9ZGt7Tfd3/AbB19zv8DQAA```',
    #     '```H4sIAAAAAAAAC42ZPYpUQRSFr6YGGpgKZWJsaKTVCIKZS3jCLGAMTAz0MpFhmZlNRUYGDS7AcgMDbkAGMXMHgqA9vvMwOPXRk3T3fJxz+9TPrerui18XX6/F9Xj17eTTycu7T/Ljuwc3v7/4fRoRz9v5jR8f7u3Ozu/8fHz7zdPXcfgru4h8GPH+8Pjo3/PD4//srWEBugDdqHOdmNMl6BJ0AboAnbKfAnsG7D6wW8Au65x1YBXY1f8nc+TyBeQLyBeQLyBfQL6AfAH5tJZcPjGXT8zlE3P5xFw+MZdPzOUTc/m05l0+MZdPzOUTs/MX6+vPc1bHnHVgl44NqDeg3oB6Y14voV5CvYR6eUw9N7eqZ9bEVg+YW4Nibm7F7JoYsJbGfA1u8+d6D+Tb5g+Y7SGQT8z2LMgnZvMF5AvIF5AvIF9AvoB8Aflink9za8/ildkzFXRBugBdgK7GvIeszPaCldkesjLbC6BeQr2Eegn1BtQbUG9AvQH1tnPajctuXm87p4G5egXqFahXoF6Betvcun2rMauwloDZfbsyu29XZvftyuy+XRmd/fZcgXzb2gXm8tUv8/cp5vItoFtAN8Z8PMXsHWzM84nZO9iYj6eYvWOOeT4xGhe35sXcmhdza76CZwXPCp4FPAt4FvAM8AzwDPDUuLj+UoEVYAFsW4PAbJ/XGgSdG5f9+l7cuSnmzs0Oug66BroGugRdgk4Z3B7rwBqwBKZ6br+LuT4h5vpLB88Onh08G3g28GzgmeCZ4JngqXFxPVnM9fIOug66BroGugRdHqGzn4/gHGuga6DroOug24Nuf4SOeo/rWds+Mr2ug2cHzw6eDTwbeDbwTPBM8Ezw3PYD5HOsAUtgOqvs/RPWfAFdAV0FXQXdAroFdNudAZi9z0OvK+BZwLOAZwXPCp4VPBfwXMBzAU+Ni/3eG5h09vMK9JcCugK6Cu9lAaa7lP1tYmX2ewa4o4jZ3xFAV0BXQVdBt4BuAZ2yH74buOJ/ny+7P4jJMIxQGgAA```',
    #     'H4sIAAAAAAAACzv369xFRgYhhvYZqjMOBV9xXOqf6LBtzQcFVgYGhkrb4NXs5ul+jZu2NLc2POBgBIp1v1+64dnKXx79YV9sZf6rOLEAxYQFkw8+P/vCr59D6v55d2EzkLrnp1MPXsg+6zg/+tZP1yfyT5mBYhKLDc/oSHI7LKx8xrLi96wdIL3b5OadX7dnk3vf9KXf68vX72ACih35+Pne40M97k2b/lanuxy6yQYU0zt67L7wt1lue1pX7ZXYrrwe5L67hd/uq33I8p+8wbe1kJf/DMi850uNXr+97O04cYnA1LdrqhTYgWIX3xq/3fljg8tcnt3yCTK7joDs0BGe+86Ltdh3QfnZ+at9muNBeqd2+X94HzXTvUnglpR66upckD/8vrP8rvrr7DRpl8HZ1QtbS0DqLvuv+79bbrfrSvdF6wTaI11B6q7E/RHZ6Szk3ZAYs1iqhEUOZK/4ST1d2YWavjMrHyWlsMWfAtnLwGDjyMDgYM/AoHoIQjfYQWhkOWEUORawXBNWOQ4UOWUscjAzJbHYVwGUuwBkiwHlGEBidlAaCHKAcg1AtswhiB4QuwEqlwCVkz+EEG+w4wHLhWDVx4GiTxJFH8R/JUjhAtZnh5DLAMk5MDAIwdwJxhC5CKAcAw45C6h9YljkwG4B2qECc4sdwn8OUDkZFDlUfSIochD/NWDVBwmXFqBcgAPCLQ4w90D9EIDkBwckd4L0bQDylaByCWA5DricBFCfFIocA4o+ISxyoPDscUCXQ4SZBJI7keUqoHLY9LXgCWuYnBSKHA/cLQ32uOMWl1wFVjnUeBDBE7ciWOLIAWuaQDVTBUuayMDj9wg8ch545CqIkJPCIyeHR04Jj1uwmZmBVR8HXC7BAbc+kJwcFn0RWOV4UORQzYTEXwm0zJKElhMM0LICBHKgZQFMDrl8gcnJY5ELwaMvBI8+kP8+IMXRATuEH1qg5RlMrgGpbIXJyaHIsaDIKWGRs8AqxwCPP5CcGha5CqicEB592ORaoH4XwxIPGXjKXVAZqeEAKQdB5TaI7eAAAMIrwORsCAAA',
    #     'H4sIAAAAAAAACzv369xFRgYmBgZ99reun567zFXNktky4/QqZgYGBqfpDQJHCla6Tg7QeiHKeosdJMbA0OMIJOwZGBQcIDQEc4DlpgDlDuCQm+MIEcelD5eZcwjI4bIP2S0g+gBYDgBWQcao8AAAAA==',
    #     'H4sIAAAAAAAAC42Wf2gcRRTHJ+epZ7yS0EorBWULQopYTOsfucp5MxvqxYqx9A+r1lI2FImgVWxQa7HxNKBIEE+t2l+ai4mVGppcjUnPpGY3KlYU49lSqELgaEAFi41oy/WiPWd2djZ37JuXHiy3+z773vu+t293Zqo09VMNWUx+ptmX55bTZF/x9kaj9dG2qwghkS2lrrg5tO69rkVf9g+07Axx2w09e14PF89sGDl1T3zk2a/Ha7htzYFnDuf3P2iOvbG7s6N3Nipsc20Hcx8uvau1/85DDzfcuiYe5rabN8580XPou5a9DTuv3/7n+f3Clmy/7dcfp+xkt1FcFvvrpLFa5BheUjI+6Gsd72scXjJ4+KyIt+eXc+Glv124O7M18kOktmbFMm5b/1C6jrVPNO89ly3kRz9+9UZu237t5OqGd07fO/DU5quP9J/cLTTHxk80jf99NvnKHY+fGZxZ1ClsB8iltclPB9elZm46+k3HNbeIHJMfdW/YeDDbvC+aTsQ3fz8sbM/VNz+wY9Oq9e9e99rE3KU/3q/jtrdiR7bV9rS0HDt+Ymzs24sDIt4j01tfKMWW3/fJ2n0jO2rffFrU1tbR21kearq/K9ze/eSp/Feip4QUGSEOJWSW/6cS/ODnhLqI5BzJiK1nK80gy/JYGX6esaWNUXmP+FkIM7yYEVMy104jvs6sx4TdcvNFXTbr1jBaalJ+NLupnJB+WTfm4GnbVmyUM0GeuDDk5YuZOdcmawi77LzXF8euZFFfJxP32ZU6hV+IWI6sqZoJP3k/zFQvoJi8V47XY1vq8PvNfwxhKVvP/JhUzxjTswzCCgirN/WsEWB+fYBOxSCdikE6FYN0KgbpVAzS6fca0KkYpFMxSKdikE7FIJ2KgTrVNTRLDJkXhBUQVnmtY2BfKNIXivSFIn2hyJwxvU7FwDljyJwxZM4YMmdMr9N/DtB7i+hUDHxvEZ2Kge8totOfCUCnYpBOxSCdikE6FYN0KhbU+XzYch5bYdHGi3Vm4h+b5pudxO9vv+itOXnuZ/D70vxffJNJxXqkWB5g6StgUEzjClgw5r9lsT4txIIxFQtJO5gPZmmE5RdkMrauPoiltWy+PshPrMUwC/F1fSEG1yBjQizk5wv2ep5BM6G0wHOWubyFevswyv77vKovemYgjFA9U/ksps8HMQNhKh8WMwuwPMIMhKl8WMxZRAvEDISpfBCzHD1z7IX9oBoUg/qpGPRsVT4opmJQTMXgedHv59MYc7gvnd/rO5X5hI6KvbBkYd/Povy9nwj6iXdMfNdXejHlIf0ctmt6kl/H3TVg1/RLFVqKLrtcLrs2j3l+Rc8vxfTMQZhhBhlxJCvYlVq8WSJ6lkL8ch5LVdUQrfKDaicI82v/LFiD6ieoE2EEYeI7aIlnVvXcpZv4TutYzpEzU72Hi1SxDMIKCKs3g0zMguP3bH5PIetDGNEz0YteT2eByfMC+x8zfz9PEBEAAA==',
    #     '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',
    # ]
    # for slab in slabs:
    #     ss = GetSlabSize(slab)
    #     print("Got slab size: {0}".format(ss))
    #     CreateSlab(slab)
    #     MoveCamera(ss['x'], 0, 0, False)
    #     sleep(0.6)








    # 
    # sleep(0.1)
    # print(CreateSlab(stairs_slab))
    # MoveCamera(5, 0, 0, False)
    # sleep(0.1)
    # print(CreateSlab(stairs_slab))
    # MoveCamera(5, 0, 0, False)
    # sleep(0.1)
    # print(CreateSlab(stairs_slab))
    # MoveCamera(0, 0, 5, False)
    # sleep(0.1)
    # print(CreateSlab(stairs_slab))
    # MoveCamera(0, 0, 5, False)
    # sleep(0.1)
    # print(CreateSlab(stairs_slab))

if __name__ == "__main__":
    main()
