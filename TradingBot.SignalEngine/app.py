from flask import Flask, request, jsonify
import openai
import os

app = Flask(__name__)

# Load OpenAI API Key from environment variable
openai.api_key = os.getenv("OPENAI_API_KEY")

@app.route('/signal', methods=['POST'])
def generate_signal():
    data = request.json
    stock_data = data.get("stock_data", "")
    preferences = data.get("preferences", {})

    try:
        # Example OpenAI call (replace prompt logic with real data and logic)
        response = openai.ChatCompletion.create(
            model="gpt-4",
            messages=[
                {
                    "role": "system",
                    "content": "You are a financial trading assistant. Generate buy/sell/hold signals."
                },
                {
                    "role": "user",
                    "content": f"Stock data: {stock_data}, Preferences: {preferences}"
                }
            ],
            temperature=0.3
        )

        ai_reply = response['choices'][0]['message']['content']
        return jsonify({"signal": ai_reply}), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "Signal engine is running."}), 200

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001)
