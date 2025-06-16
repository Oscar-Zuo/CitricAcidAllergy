import json
import random
import boto3
from decimal import Decimal
from boto3.dynamodb.conditions import Key

dynamodb = boto3.resource('dynamodb')
table = dynamodb.Table('GamJamReinforcement')


def lambda_handler(event, context):
    try:
        # Parse the request body
        body = event.get("body")
        if isinstance(body, str):
            body = json.loads(body)

        target_level = body.get("level")
        if target_level is None:
            raise ValueError("Missing required field: 'level'")

        target_value = body.get("value")  # optional

        # Try to get item
        item = get_random_item_by_level(target_level, target_value)

        if item:
            return {
                "statusCode": 200,
                "body": json.dumps({"item": item}, default=str)
            }
        else:
            return {
                "statusCode": 404,
                "body": json.dumps({"message": "No item found at level or fallback."})
            }

    except Exception as e:
        return {
            "statusCode": 400,
            "body": json.dumps({"error": str(e)})
        }


def get_random_item_by_level(target_level, target_value=None):
    item = query_level(target_level, target_value)
    if item:
        return item
    return query_level(1, target_value)


def query_level(level, target_value=None):
    kwargs = {
        'IndexName': 'level-value-index',
        'KeyConditionExpression': Key('level').eq(level)
    }

    if target_value is not None:
        kwargs['KeyConditionExpression'] &= Key('value').eq(Decimal(str(target_value)))

    response = table.query(**kwargs)
    items = response.get('Items', [])

    if not items:
        return None

    return random.choice(items)