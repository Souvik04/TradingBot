#!/usr/bin/env python3
"""
Health check script for the TradingBot Signal Engine
"""

import requests
import sys
import time

def check_health():
    """Check if the signal engine is healthy"""
    try:
        # Try to connect to the health endpoint
        response = requests.get("http://localhost:8000/health", timeout=5)
        
        if response.status_code == 200:
            data = response.json()
            if data.get("status") == "ok":
                print("Signal engine is healthy")
                return True
            else:
                print(f"Signal engine health check failed: {data}")
                return False
        else:
            print(f"Signal engine health check failed with status code: {response.status_code}")
            return False
            
    except requests.exceptions.ConnectionError:
        print("Cannot connect to signal engine")
        return False
    except requests.exceptions.Timeout:
        print("Signal engine health check timed out")
        return False
    except Exception as e:
        print(f"Signal engine health check error: {e}")
        return False

if __name__ == "__main__":
    # Give the service a moment to start up
    time.sleep(2)
    
    if check_health():
        sys.exit(0)
    else:
        sys.exit(1)
