import json
import uuid
import boto3
from decimal import Decimal
from datetime import datetime

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('GameJamReinforcement')

MAX_LEVEL = 10
LEVEL_SEPARATION = 8

def convert_floats_to_decimals(obj):
    if isinstance(obj, list):
        return [convert_floats_to_decimals(i) for i in obj]
    elif isinstance(obj, dict):
        return {k: convert_floats_to_decimals(v) for k, v in obj.items()}
    elif isinstance(obj, float):
        return Decimal(str(obj))
    else:
        return obj

def get_reinforcement_level(value: float) -> int:
    return min(MAX_LEVEL, int((1 + value) / LEVEL_SEPARATION))

def lambda_handler(event, context):
    try:
        body = event.get("body")
        if isinstance(body, str):
            body = json.loads(body)

        cleaned = clean_input(body)

        cleaned['uuid'] = str(uuid.uuid4())
        cleaned['timestamp'] = datetime.utcnow().isoformat()
        cleaned['level'] = get_reinforcement_level[cleaned["value"]]

        table.put_item(Item=cleaned)

        return {
            "statusCode": 200,
            "body": json.dumps({"message": "Data stored successfully", "id": cleaned['uuid']})
        }

    except Exception as e:
        return {
            "statusCode": 400,
            "body": json.dumps({"error": str(e)})
        }


def clean_input(data):
    if not isinstance(data, dict):
        raise ValueError("Input must be a JSON object.")

    data = convert_floats_to_decimals(data)

    # Required fields
    required_fields = ["senderName", "value", "turrets"]
    for field in required_fields:
        if field not in data:
            raise ValueError(f"Missing field: {field}")

    if not isinstance(data["senderName"], str):
        raise ValueError("senderName must be a string.")
    
    data["value"] = Decimal(data["value"])  # normalize to Decimal

    # Clean turrets
    if not isinstance(data["turrets"], list):
        raise ValueError("turrets must be a list.")
    
    for turret in data["turrets"]:
        if "name" not in turret or "grid" not in turret or "plugins" not in turret:
            raise ValueError("Each turret must have 'name', 'grid', and 'plugins'.")

        turret["name"] = str(turret["name"])
        turret["plugins"] = list(map(str, turret["plugins"]))

        grid = turret["grid"]
        if not isinstance(grid, dict) or "x" not in grid or "y" not in grid:
            raise ValueError("Each grid must be an object with x and y.")

        grid["x"] = int(grid["x"])
        grid["y"] = int(grid["y"])

    return data