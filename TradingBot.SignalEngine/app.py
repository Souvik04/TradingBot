from flask import Flask, request, jsonify
import openai
import os
from engine import generate_signal  # classic indicator-based

app = Flask(__name__)

openai.api_key = os.getenv("OPENAI_API_KEY")

@app.route('/signal/ai', methods=['POST'])
def generate_signal_ai():
    data = request.json
    stock_data = data.get("stock_data", "")
    preferences = data.get("preferences", {})

    try:
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

@app.route('/signal/classic', methods=['POST'])
def generate_signal_classic():
    data = request.get_json()
    symbol = data.get("symbols", [""])[0]  # Accepts array, uses first symbol
    if not symbol:
        return jsonify({"error": "No symbol provided"}), 400

    result = generate_signal(symbol)
    return jsonify(result)

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "Signal engine is running."}), 200

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001)